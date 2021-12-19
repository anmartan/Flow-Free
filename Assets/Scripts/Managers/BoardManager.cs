using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class BoardManager : MonoBehaviour
    {
        [Tooltip("Camera in the scene, used to scale the tiles of the grid.")]
        [SerializeField] private Camera _sceneCamera;           // Camera of the scene. Used for scaling the grid.

        [Tooltip("Tile prefab. The grid will be filled with these.")]
        [SerializeField] private Tile _tilePrefab;              // Prefab of the tile that will be instantiated for creating the grid.

        [Tooltip("Margin at the top of the screen, used for the HUD, in pixels.")]
        [SerializeField] private GameObject _topMargin;         // Margins left at both the top and bottom of the screen, for the HUD.

        [Tooltip("Margin at the bottom of the screen screen left unused by the board, in pixels.")]
        [SerializeField] private GameObject _bottomMargin;      // Margins left at both sides of the screen, so that the board doesn't occupy the whole screen.

        private LevelManager _levelManager;                     // Level manager instance, so that it is not necessary to call the GameManager constantly.
        
        private int _width, _height;                            // Width and height of the board (in tiles).

        private Tile[,] _tiles;                                 // Tiles array so that they can be accessed later on.

        private List<Vector2Int>[] _solution;                   // Solution to the puzzle. Containts a number of lists with the positions of the tiles in each flow.
        private bool[] _hintsGiven;

        private bool _touching;                                 // Whether the player is currently touching the board or not.        
        private bool _lastMoveChangedState;                     // Whether the last player changed the state of the aame or not.
        private Vector2Int _lastTile;                           // The last tile the player interacted with (either by pressing, moving or releasing the screen).

        private int _currentColor;                              // The _color index of the flow the player is currently touching.
        private int _intermediateColor;                         // The _color index of the flow the player changed in the intermediate state.
        private int _lastColor;                                 // The _color index of the flow the player changed in the last state.

        private List<Vector2Int>[] _currentState;               // The state of the board the player is currently changing.
        private List<Vector2Int>[] _intermediateState;          // The state of the board the player will go back to if they hit the undo button.
        private List<Vector2Int>[] _lastState;                  // The state of the board that is permanent (a.k.a. the player changed many moves ago).

        private int _usableTiles;                               // The number of tiles the player can interact with. That is, all the tiles in the board, except for those which are gaps.
        
        
        // ----- PUBLIC METHODS ----- //

        /// <summary>
        /// Creates a board the player can interact with. Creates the tiles and sets the initial circles, with the corresponding colors.
        /// Receives a map which has been previously loaded and includes the relevant information for the level.
        /// </summary>
        /// <param name="map">A map with the level information.</param>
        public void CreateBoard(Map map)
        {
            _levelManager = GameManager.Instance().GetLevelManager();
            
            
            _width = map.GetWidth();
            _height = map.GetHeight();
            _tiles = new Tile[_height, _width];
            List<Tuple<Vector2Int, Vector2Int>> walls = map.GetWalls();
            List<Vector2Int> gaps = map.GetGaps();

            _usableTiles = _width * _height - gaps.Count;
            _currentColor = -1;
            _intermediateColor = -1;
            _lastColor = -1;
            _touching = false;
            
            // Creates the lists that will contain information about the board: the final solution, the current state and the previous state.
            _solution = new List<Vector2Int>[map.GetFlowsNumber()];
            _hintsGiven = new bool[map.GetFlowsNumber()];
            
            _currentState = new List<Vector2Int>[map.GetFlowsNumber()];
            _intermediateState = new List<Vector2Int>[map.GetFlowsNumber()];
            _lastState = new List<Vector2Int>[map.GetFlowsNumber()];

            for (int i = 0; i < map.GetFlowsNumber(); i++)
            {
                _solution[i] = new List<Vector2Int>();
                _hintsGiven[i] = false;
                
                _currentState[i] = new List<Vector2Int>();
                _intermediateState[i] = new List<Vector2Int>();
                _lastState[i] = new List<Vector2Int>();
            }

            // Destroys all of the tiles it saved previously.
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

            // Resets the scale and position, so that when the new pool of objects is created, it is well scaled. 
            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;

            // Instantiates the grid, creating every tile.
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    // Creates a new tile, and assigns it to the tile in that position in the array.
                    // Instantiates it with a (0.5f, -0.5f) offset, so that the parent is in the top-left corner.
                    _tiles[i, j] = Instantiate(_tilePrefab, new Vector3(j + 0.5f, -i - 0.5f, 0), Quaternion.identity, transform);
                    _tiles[i,j].SetBackgroundColor(GameManager.Instance().GetLevelData().Color);
                }
            }

            // Sets the colors for each circle. Gets the _color according to the theme,
            // And paints the circles at both the beginning and the end of the flow with such _color.
            for (int i = 0; i < map.GetFlowsNumber(); i++)
            {
                _solution[i] = map.GetFlows()[i];

                _tiles[_solution[i][0].y, _solution[i][0].x].SetCircle(i);
                _tiles[_solution[i][_solution[i].Count - 1].y, _solution[i][_solution[i].Count - 1].x].SetCircle(i);
            }

            // Sets the gaps in the board; they will not be interactable.
            for (int i = 0; i < gaps.Count; i++)
            {
                Vector2Int position = gaps[i];
                _tiles[position.y, position.x].SetActive(false);
            }
            
            // Sets the walls in every tile; in both of the tiles, in opposite directions.
            for (int i = 0; i < walls.Count; i++)
            {
                Tuple<Vector2Int, Vector2Int> wall = walls[i];
                Vector2Int direction = wall.Item1 - wall.Item2;
                
                _tiles[wall.Item1.y, wall.Item1.x].SetWallActive(direction);
                _tiles[wall.Item2.y, wall.Item2.x].SetWallActive(-direction);
            }
            
            // Puts the board in the center, visually.
            transform.Translate(new Vector3(-_width * 0.5f, _height * 0.5f));

            // Scales the camera so that the whole content fits in.
            int size = (_width >= _height) ? _width : _height;
            _sceneCamera.orthographicSize = size + _topMargin.transform.lossyScale.y + _bottomMargin.transform.lossyScale.y;
                                            
            /*
              
            float size = _height * 0.5f + _topMargin.transform.lossyScale.y + _bottomMargin.transform.lossyScale.y;
            if (_sceneCamera.aspect * size < _width)
            {
                size = (_width * 0.5f / _sceneCamera.aspect);
            }
            _sceneCamera.orthographicSize = size * 2;
            */
        }
        
        /// <summary>
        /// What happens when the user starts touching the board: a new flow is to be drawn.
        /// </summary>
        /// <param name="touch">The position of the touch.</param>
        public void OnTouchStarted(Vector3 touch)
        {
            StartFlow(GetTileCoordinates(touch));
            _touching = true;
        }

        /// <summary>
        /// What happens when the player moves the finger through the board: if possible, the flow continues in the direction of the movement.
        /// </summary>
        /// <param name="touch">Current position of the touch.</param>
        public void OnTouchMoved(Vector3 touch)
        {
            // If the touch started outside the board, nothing happens.
            if(!_touching) return;
            
            // Calculates which tile the touch corresponds to, and moves the flow.
            ContinueFlow(GetTileCoordinates(touch));
        }

        /// <summary>
        /// What happens when the player stops touching the board: the state is saved, the number of movements updated, etc.
        /// </summary>
        public void OnTouchFinished()
        {
            // If the touch started outside the board, nothing happens.
            if(!_touching) return;
   
            // If the hint was previously given and the flow is recovered, the stars are painted.
            if(_hintsGiven[_currentColor] && !StateChanged(_currentColor, _solution, _currentState)) SetStarsActive(_currentColor, true);

            // Updates the information of the levelManager.
            UpdateCompleteFlows();
            
            // If there was any change in the state, the state has to be saved.
            if (StateChanged(_currentColor, _currentState, _lastState))
            {
                OnStateChanged();
                return;
            }
            
            _currentColor = _lastColor;
            _touching = false;
        }
        
        /// <summary>
        /// Undoes the last movement.
        /// </summary>
        public void UndoMovement()
        {
            foreach (List<Vector2Int> list in _currentState)
            {
                foreach (Vector2Int tile in list)
                {
                    // Clears the tile completely.
                    _tiles[tile.y, tile.x].Dissolve(false);
                }
            }
            
            // If the movement undone was a movement, it is also undone.
            if (_currentColor != _lastColor) _levelManager.UpdateMovements(-1);
            
            // Restores the last state.
            SaveState(_lastState, _currentState);
            
            // If the last move changed the state, the intermediate state is also restored.
            if (_lastMoveChangedState) SaveState(_lastState, _intermediateState);

            // Draws the current state, once restored, to recover the visual state.
            for (int i = 0; i < _currentState.Length; i++) 
            {
                // If the state recovered is the same as the given with a clue, puts the stars back on.
                if (_hintsGiven[i] && !StateChanged(i, _solution, _currentState)) SetStarsActive(i, true);
                else SetStarsActive(i, false);
                
                // Draws every flow.
                for (int j = 0; j < _currentState[i].Count; j++)
                {
                    Vector2Int pos = _currentState[i][j];
                    Tile tile = _tiles[pos.y, pos.x];
                    tile.SetColor(i);
                    if (j < _currentState[i].Count - 1)
                    {
                        Vector2Int nextPos = _currentState[i][j + 1];
                        Vector2Int dir = pos - nextPos;
                        CrossFlow(pos, dir);
                    }
                }
            }
            _lastMoveChangedState = false;
            
            // Updates the information on the UI.
            UpdatePercentage();
            UpdateCompleteFlows();
        }
        
        /// <summary>
        /// Tries to give a hint to the player. If every hint has already been given, this method will do nothing.
        /// </summary>
        /// <returns>true if a clue was given; false if all of the hints have already been given.</returns>
        public bool UseHint()
        {
            int _lastHintGiven = 0;

            // Looks for a hint that has not been given.
            // If it has not been given, but the player has already found the solution to a flow, that hint is ignored.
            for (int i = 0; i < _solution.Length; i++)
            {
                if (!_hintsGiven[i] && StateChanged(i, _solution, _currentState)) break;

                _lastHintGiven++;
            }
            
            // If no possible hint was given, returns false.
            if (_lastHintGiven >= _solution.Length) return false;
            
            // Activates the clue so that it is not given again.
            _hintsGiven[_lastHintGiven] = true;
            
            // Draws the flow completely, undoing other flows if they are on the way.
            StartFlow(_solution[_lastHintGiven][0]);
            for (int j = 1; j < _solution[_lastHintGiven].Count; j++) ContinueFlow(_solution[_lastHintGiven][j]);

            _lastMoveChangedState = true;
            OnStateChanged();
            
            // Draws the stars.
            SetStarsActive(_lastHintGiven, true);
            return true;
        }
        
        
        // ----- PUBLIC INFO ENQUIRY METHODS ----- //
        
        /// <summary>
        /// Checks whether the position given corresponds to the board or not. Used to filter input.
        /// </summary>
        /// <param name="position"> The position that is to be checked.</param>
        /// <returns>true if the position given is inside the board boundaries; false if the position does not correspond to the board.</returns>
        public bool InsideBoundaries(Vector3 position)
        {
            return (position.x > transform.position.x && position.x < transform.position.x + _width &&      // x axis
                    position.y < transform.position.y && position.y > transform.position.y - _height);      // y axis
        }

        /// <summary>
        /// Checks whether the level has finished: if every flow is the same as in the original solution, it has finished.
        /// </summary>
        /// <returns>true if the level has finished; false otherwise.</returns>
        public bool LevelFinished()
        {
            // Checks that every tile is in the same flow as in the solution.
            for(int i = 0; i < _solution.Length; i++)
            {
                for(int j = 0; j < _solution[i].Count; j++)
                {
                    Vector2Int tile = _solution[i][j];
                    
                    // If the tile is not the same color, or is not connected to other tiles, the level has not finished.
                    if (_tiles[tile.y, tile.x].GetColorIndex() != i || !_tiles[tile.y, tile.x].IsFullyConnected()) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether the state changed with the last movement.
        /// </summary>
        /// <returns>true if the state changed; false otherwise.</returns>
        public bool GetStateChanged() { return _lastMoveChangedState; }
        
        
        // ----- PRIVATE METHODS ----- //
        
        /// <summary>
        /// What happens when the state of the board changed with the last movement.
        /// The state is saved (if there were previous actions, they are no longer undoable).
        /// Updates the number of movements, if necessary.
        /// </summary>
        private void OnStateChanged()
        {
            // If the state was changed in the last movement, it has to be saved before.
            // This action will no longer be able to be undone.
            if (_lastMoveChangedState)
            {
                SaveState(_intermediateState, _lastState);
                _lastColor = _intermediateColor;
                _lastMoveChangedState = false;
            }

            // The intermediate state is saved: the player will be able to come back to this one, if they hit the undo button.
            SaveState(_currentState, _intermediateState);
            _intermediateColor = _currentColor;
            _lastMoveChangedState = true;

            // If the change is considered a movement, the number of movements increases.
            if (_currentColor != _lastColor) _levelManager.UpdateMovements(1);
        }

        /// <summary>
        /// Starts a flow in the given tile.
        /// Takes this into consideration:
        /// If an empty tile is touched, nothing happens.
        /// If a tile is touched and there was a flow coming out from that tile, the rest of the flow has to be dissolved.
        /// </summary>
        /// <param name="pos">Position of the tile that will start the flow.</param>
        private void StartFlow(Vector2Int pos)
        {
            Tile tile = _tiles[pos.y, pos.x];

            // If the tile does not have a color, it cannot start a flow.
            int color = tile.GetColorIndex();
            if (color == -1) return;
            
            // If the tile touched is a circle, the whole flow is dissolved.
            if (tile.IsCircle()) DissolveFlow(color);
                
            // If the tile touched was not the ending part of a flow, dissolves anything that came after it.
            else if (_currentState[color].Count > 0) DissolveFlow(color, pos);

            _currentColor = color;

            // Updates the percentage of flows occupied
            UpdatePercentage();
            
            // Add this flow to the list of changes.
            AddFlow(pos);

            _lastTile = pos;
        }

        /// <summary>
        /// Makes the current flow grow to the given tile.
        /// Takes this into consideration:
        /// If it is not a valid movement (for any reason), nothing happens.
        /// If the flow cuts a part of itself, the flow has to be correctly dissolved.
        /// If the flow cuts another flow, it has to be correctly dissolved.
        /// The previous state cannot be destroyed, in case the player wants to undo their last movement.
        /// </summary>
        /// <param name="pos">The tile to which the player moved.</param>
        private void ContinueFlow(Vector2Int pos)
        {
            Tile tile = _tiles[pos.y, pos.x];
            
            // If the movement did not result in a tile change, nothing happens.
            if(pos == _lastTile) return;
            
            // If it is not a valid movement, there is nothing to do.
            if (!ValidMovement(pos)) return;
            
            // Calculates the direction of the movement.
            Vector2Int direction = pos - _lastTile;

            // If there is a flow prior to this movement, some part of the flow has to be dissolved.
            int previousColor = tile.GetColorIndex();
            if (previousColor != -1)
            {
                int positionInList = _currentState[previousColor].IndexOf(pos);

                // If the flow is the same color, and it is not the missing circle, it is removed until this position.
                if (previousColor == _currentColor && !tile.IsCircle() || pos == _currentState[_currentColor][0])
                {
                    List<Vector2Int> dissolvedTiles = DissolveFlow(previousColor, _currentState[previousColor][positionInList]);
                        
                    // If there was a flow in this position, it is restored.
                    for(int i= 0; i < dissolvedTiles.Count;i++)
                    {
                        RestoreFlow(dissolvedTiles[i]);
                    }
                }
                // If it is another color, though, it has to be dissolved until the previous position, so that both flows do not share this tile.
                else if (previousColor != _currentColor) DissolveFlow(previousColor, _currentState[previousColor][positionInList - 1]);
            }

            // If the flow wasn't a part of the flow, it is drawn.
            if(AddFlow(pos))    CrossFlow(pos, direction);
            _lastTile= pos;
            
            // Updates the percentage of flows occupied
            UpdatePercentage();
        }

        /// <summary>
        /// Adds the tile to the current flow, if possible.
        /// </summary>
        /// <param name="lastTile">The tile to be added.</param>
        /// <returns>true if the tile was added; false if the tile was already a part of that flow.</returns>
        private bool AddFlow(Vector2Int lastTile)
        {
            return AddFlow(_currentColor, lastTile);
        }
        
        /// <summary>
        /// Adds the tile to a specific flow, if possible.
        /// </summary>
        /// <param name="colorIndex">The index of the flow to which the tile will be added.</param>
        /// <param name="lastTile">The tile that will be added.</param>
        /// <returns>true if the tile was added; false if the tile was already a part of that flow.</returns>
        private bool AddFlow(int colorIndex,Vector2Int lastTile)
        {
            // Finds the flow with that index.
            List<Vector2Int> flow = _currentState[colorIndex];

            // If the tile is that flow, it cannot be added again.
            if (flow.Contains(lastTile)) return false;

            // Adds the tile to the flow, and sets its color.
            flow.Add(lastTile);
            _tiles[lastTile.y, lastTile.x].SetColor(colorIndex);

            return true;
        }
        
        /// <summary>
        /// Makes a bridge appear between the tile in the given position, and the next tile in the given direction.
        /// </summary>
        /// <param name="position"> Position of the tile where the bridge will be added.</param>
        /// <param name="direction">The direction in which the flow will be expanded.</param>
        private void CrossFlow(Vector2Int position, Vector2Int direction)
        {
            // Puts the flow in the tile, in the given direction.
            Tile tile = _tiles[position.y, position.x];
            tile.SetFlowActive(direction, true);
            
            // Puts the flow in the next tile, in the opposite direction.
            tile = _tiles[position.y - direction.y, position.x - direction.x];
            tile.SetFlowActive(-direction, true);
        }

        /// <summary>
        /// Dissolves the whole flow in that index. 
        /// </summary>
        /// <param name="colorIndex">The index of the flow that will be dissolved.</param>
        private void DissolveFlow(int colorIndex)
        {
            DissolveFlow(colorIndex, new Vector2Int(-1, -1));
        }
        
        /// <summary>
        /// Dissolves the flow in that index, from a point onwards.
        /// </summary>
        /// <param name="colorIndex">The index of the flow that will be dissolved.</param>
        /// <param name="lastIncluded">The position of the last tile that will be preserved. Everything from this point onwards will be dissolved. </param>
        /// <returns> A list with all the tiles that have been dissolved.</returns>
        private List<Vector2Int> DissolveFlow(int colorIndex, Vector2Int lastIncluded)
        {
            // If it is not a valid _color, there is nothing to do.
            if(colorIndex < 0 ) return null;

            List<Vector2Int> dissolvedTiles = new List<Vector2Int>();

            // Removes the stars from the tips of the flow, if there were any.
            SetStarsActive(colorIndex, false);
            
            List<Vector2Int> flow = _currentState[colorIndex];
            int positionInList = flow.IndexOf(lastIncluded);

            // If the touched tile is in the list, the whole list is not removed; only the part that came after touching that tile.
            if (positionInList != -1) _tiles[lastIncluded.y, lastIncluded.x].Dissolve(true);

            // Dissolves the rest of the tiles from this point on, and stores them in the list of changes
            for (int i = positionInList + 1; i < flow.Count; i++)
            {
                Vector2Int pos = flow[i];
                Tile tile = _tiles[pos.y, pos.x];
                dissolvedTiles.Add(pos);

                tile.Dissolve(false);
            }
            flow.RemoveRange(positionInList + 1, flow.Count - (positionInList + 1));

            return dissolvedTiles;
        }

        /// <summary>
        /// Tries to restore the flow that appeared in the last state in the given position.
        /// Used when the player is moving the finger over an existing flow (and therefore breaks it), but then moves back.
        /// </summary>
        /// <param name="tilePosition"> Position of the tile that will be restored.</param>
        private void RestoreFlow(Vector2Int tilePosition)
        {
            int i = 0;

            // Finds the flow that existed in the intermediate state, before cutting it.
            while(i < _intermediateState.Length)
            {
                if (_intermediateState[i].Contains(tilePosition)) break;
                i++;
            }
            
            // If there was no flow, nothing can be restored.
            if (i >= _intermediateState.Length) return;

            // If it is not connected to the actual flow, nothing to do.
            int lastTile = _currentState[i].Count - 1;
            int currentTile = _intermediateState[i].IndexOf(tilePosition);
            
            if (currentTile - lastTile > 1) return;
            currentTile--;

            // Starts running over the intermediate flow, restoring every possible tile.
            while (currentTile < _intermediateState[i].Count - 1)  
            {
                Vector2Int pos = _intermediateState[i][currentTile];
                Tile tile = _tiles[pos.y, pos.x];
                Vector2Int nextPos = _intermediateState[i][currentTile + 1];
                Vector2Int dir = pos - nextPos;

                // If the tile is being used by the current flow, it cannot be restored.
                if(_currentState[_currentColor].Contains(pos)) return;
                
                // Adds the tile to the previous flow.
                tile.SetColor(i);
                AddFlow(i, pos);

                // If the next tile is being used by the current flow, the previous one cannot grow.
                if (!_currentState[_currentColor].Contains(nextPos)) CrossFlow(pos, dir);
                else return;
                
                currentTile++;
            }
            AddFlow(i, _intermediateState[i][currentTile]);
        }
        
        /// <summary>
        /// Copies the state from one list to another. Used to save the current state into the intermediate one, and
        /// from that one to the lastState one.
        /// </summary>
        /// <param name="from">List that will be copied.</param>
        /// <param name="to">List that will be overwritten.</param>
        private void SaveState(List<Vector2Int>[] from, List<Vector2Int>[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                // Completely erases the list that will be overwritten.
                to[i].Clear();
                for (int j = 0; j < from[i].Count; j++)
                {
                    // Adds every item from the list that is copied to the other list.
                    to[i].Add(new Vector2Int(from[i][j].x, from[i][j].y));
                }
            }
        }
        
        /// <summary>
        /// Updates the percentage of tiles currently used, and calls the Level Manager to show the updated number.
        /// </summary>
        private void UpdatePercentage()
        {
            float tilesFlowing = 0;
            for (int i = 0; i < _currentState.Length; i++) tilesFlowing += _currentState[i].Count;
            
            _levelManager.UpdatePipePercentage((int)(tilesFlowing * 100 / _usableTiles));
        }

        /// <summary>
        /// Updates the number of flows that are completely connected, and calls the Level Manager to show the updated number.
        /// </summary>
        private void UpdateCompleteFlows()
        {
            int completeFlows = 0;
            for (int i = 0; i < _currentState.Length; i++)
            {
                if (_currentState[i].Contains(_solution[i][0]))// &&
                    if(_currentState[i].Contains(_solution[i][_solution[i].Count - 1]))
                         completeFlows++;
            }
            
            _levelManager.UpdateFlowsText(completeFlows);
        }
        
        
        // ----- PRIVATE INFO ENQUIRY METHODS ----- //
        
        /// <summary>
        /// Compares the flow with the given index, between two states, to see if there is any change among them.
        /// </summary>
        /// <param name="colorIndex"> The index of the flow that will be compared.</param>
        /// <param name="listA">One of the states to compare with.</param>
        /// <param name="listB">The other state to compare with.</param>
        /// <returns>true if there is a change between the two states; false if the flow remained the same.</returns>
        private bool StateChanged(int colorIndex, List<Vector2Int>[] listA, List<Vector2Int>[] listB)
        {
            // If the flow only has one element, it is considered to be unchanged (as it has no effect in the game).
            if (listA[colorIndex].Count <= 1 && listB[colorIndex].Count <= 1) return false;

            // If the current flow and the previous one have different sizes, there has been some changes.
            if (listA[colorIndex].Count != listB[colorIndex].Count) return true;

            // If the flow changed the start, there may have been a change.
            int startIndex = listB[colorIndex].IndexOf(listA[colorIndex][0]);

            // If the previous flow does not contain the new one's start, the flow changed.
            if (startIndex == -1) return true;

            // If it is included, it needs to be in the same order (or inversed).
            int size = listA[colorIndex].Count;
            if (startIndex == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (listA[colorIndex][i] != listB[colorIndex][i]) return true;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    if (listA[colorIndex][startIndex - i] != listB[colorIndex][i]) return true;
                }
            }

            // If everything else did not change, it is safe to assume the state did not change.
            return false;
        }
        
        /// <summary>
        /// Checks that the movement is a valid one: that is, that the current tile is connected to the last one,
        /// and that the movement does not break any rule.
        /// </summary>
        /// <param name="pos">Position of the tile that will be checked.</param>
        /// <returns>true if the movement is valid; false if the flow cannot grow onto that tile.</returns>
        private bool ValidMovement(Vector2Int pos)
        {
            // If it is a gap, the player cannot move there.
            if (_tiles[pos.y, pos.x].IsGap()) return false;
            
            // If there is more than one tile of distance between them, the movement is not valid
            if ((_lastTile - pos).magnitude > 1) return false;

            int flowIndex = _tiles[_lastTile.y, _lastTile.x].GetColorIndex();

            // If there is a circle of another _color in the actual tile, the flow cannot grow.
            if (_tiles[pos.y, pos.x].IsCircle() && flowIndex != _tiles[pos.y, pos.x].GetColorIndex()) return false;

            // If there is a wall between the last tile and the current position, the flow cannot grow.
            if (_tiles[pos.y, pos.x].IsWallActive(pos - _lastTile)) return false;
            
            // If the flow has both circles covered, and the movement is not used to undo the flow, it is not a valid movement.
            if (_currentState[flowIndex].Contains(_solution[flowIndex][0]) &&                               // Contains the first circle
                _currentState[flowIndex].Contains(_solution[flowIndex][_solution[flowIndex].Count - 1]) &&  // Contains the second circle
                !_currentState[flowIndex].Contains(pos))                                                    // Is not an "undo" movement
                return false;

            // If everything else was correct, the movement is a valid one.
            return true;
        }

        /// <summary>
        /// Returns the row and column of the tile that is in touchPosition, in world units.
        /// </summary>
        /// <param name="position">The position of the tile, in world units.</param>
        /// <returns>The position of the tile, in the array of tiles. (column, row)</returns>
        private Vector2Int GetTileCoordinates(Vector3 position)
        {
            // Removes the offset, so that the touch is in the range [(0,0), (_width, _height)]
            position -= transform.position;

            // Calculates the tile clicked, using the integer part of each component of the vector
            int row = Mathf.Clamp(Mathf.FloorToInt(-position.y), 0, _height - 1);
            int column = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, _width - 1);

            return new Vector2Int(column, row);
        }
        
        /// <summary>
        /// Changes the visibility of the stars in a given flow.
        /// </summary>
        /// <param name="colorIndex">Flow that will activate or deactivate the stars.</param>
        /// <param name="active">Whether the stars shall remain active or not.</param>
        private void SetStarsActive(int colorIndex, bool active)
        {
            Vector2Int starTile = _solution[colorIndex][0];
            _tiles[starTile.y, starTile.x].SetStarActive(active);

            starTile = _solution[colorIndex][_solution[colorIndex].Count - 1];
            _tiles[starTile.y, starTile.x].SetStarActive(active);
        }
    }
    
}
