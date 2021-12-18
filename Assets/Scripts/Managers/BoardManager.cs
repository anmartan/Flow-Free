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

        [Tooltip("Instance of the Level Manager, so that it is not necessary to call the Game Manager in order to access it.")]
        [SerializeField] private LevelManager _levelManager;    // Level manager instance, so that it is not necessary to call the GameManager constantly.
        
        private int _width, _height;                            // Width and height of the board (in tiles).

        private Tile[,] _tiles;                                 // Tiles array so that they can be accessed later on.

        private List<Vector2Int>[] _solution;                   // Solution to the puzzle. Containts a number of lists with the positions of the tiles in each flow.
        private bool[] _hintsGiven;

        private bool _touching;                                 // Whether the player is currently touching the board or not.        
        private bool _lastMoveChangedState;                     // Whether the last player changed the state of the aame or not.
        private Vector2Int _lastTile;                           // The last tile the player interacted with (either by pressing, moving or releasing the screen).

        private int _currentColor;                              // The color index of the flow the player is currently touching.
        private int _intermediateColor;                         // The color index of the flow the player changed in the intermediate state.
        private int _lastColor;                                 // The color index of the flow the player changed in the last state.

        private List<Vector2Int>[] _currentState;               // The state of the board the player is currently changing.
        private List<Vector2Int>[] _intermediateState;          // The state of the board the player will go back to if they hit the undo button.
        private List<Vector2Int>[] _lastState;                  // The state of the board that is permanent (a.k.a. the player changed many moves ago).

        private int _usableTiles;
        
        /// <summary>
        /// Creates a board the player can interact with. Creates the tiles and sets the initial circles, with the corresponding colors.
        /// Receives a map which has been previously loaded and includes the relevant information for the level.
        /// </summary>
        /// <param name="map">A map with the level information.</param>
        public void CreateBoard(Map map)
        {
            // Updates the information of the LevelManager.
            _levelManager.ResetMovements();
            _levelManager.UpdateFlowsText(0);
            _levelManager.UpdatePipePercentage(0);
            
            _width = map.getWidth();
            _height = map.getHeight();
            _tiles = new Tile[_height, _width];
            List<Tuple<Vector2Int, Vector2Int>> walls = map.GetWalls();
            List<Vector2Int> gaps = map.GetGaps();

            _usableTiles = _width * _height - gaps.Count;
            
            // Creates the lists that will contain information about the board: the final solution, the current state and the previous state.
            _solution = new List<Vector2Int>[map.getFlowsNumber()];
            _hintsGiven = new bool[map.getFlowsNumber()];
            
            _currentState = new List<Vector2Int>[map.getFlowsNumber()];
            _intermediateState = new List<Vector2Int>[map.getFlowsNumber()];
            _lastState = new List<Vector2Int>[map.getFlowsNumber()];

            for (int i = 0; i < map.getFlowsNumber(); i++)
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
                }
            }

            // Sets the colors for each circle. Gets the color according to the theme,
            // And paints the circles at both the beginning and the end of the flow with such color.
            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                _solution[i] = map.getFlows()[i];

                _tiles[_solution[i][0].y, _solution[i][0].x].PutCircle(i);
                _tiles[_solution[i][_solution[i].Count - 1].y, _solution[i][_solution[i].Count - 1].x].PutCircle(i);
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
            // If the touch started outside the board, nothing happens
            if(!_touching) return;
   
            // If the hint was previously given and the flow is recovered, the stars are painted
            if(_hintsGiven[_currentColor] && !StateChanged(_currentColor, _solution, _currentState)) SetStarsActive(_currentColor, true);

            // Updates the information of the levelManager
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
                    _tiles[tile.y, tile.x].ResetState();
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
            
            // Updates the information on the UI
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
        
        // - GETTERS -
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
        
        // - PRIVATE - //
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
            if (IsMovement()) _levelManager.UpdateMovements(1);
        }

        private void StartFlow(Vector2Int pos)
        {
            // If the touch has just started, take this into consideration:
            // If an empty tile is touched, nothing happens.
            // If a tile is touched and there was a flow coming out from that tile, the rest of the flow has to be dissolved.

            Tile tile = _tiles[pos.y, pos.x];

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
        /// If the touch is moving, take this into consideration:
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
            Vector2Int direction = pos - _lastTile;

            // If there is a flow prior to this movement, some part of the flow has to be dissolved.
            int previousColor = tile.GetColorIndex();
            if (previousColor != -1)
            {
                int positionInList = _currentState[previousColor].IndexOf(pos);

                // If the flow is the same color, and it is not the missing circle it is removed until this position.
                if (previousColor == _currentColor && !tile.IsCircle() || pos == _currentState[_currentColor][0])
                {
                    DissolveFlow(previousColor, _currentState[previousColor][positionInList]);
                        
                    // If there was a flow in this position, it is restored.
                    RestoreFlow(_lastTile);
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
        /// Returns the row and column of the tile that is in touchPosition, in world units.
        /// </summary>
        /// <param name="position">The position of the tile, in world units.</param>
        /// <returns>The position of the tile, in the array of tiles. (column, row)</returns>
        private Vector2Int GetTileCoordinates(Vector3 position)
        {
            // Removes the offset, so that the touch is in the range [(0,0), (_width, _height)]
            position = position - transform.position;

            // Calculates the tile clicked, using the integer part of each component of the vector
            int row = Mathf.Clamp(Mathf.FloorToInt(-position.y), 0, _height - 1);
            int column = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, _width - 1);

            return new Vector2Int(column, row);
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
        private void DissolveFlow(int colorIndex, Vector2Int lastIncluded)
        {
            // If it is not a valid color, there is nothing to do.
            if(colorIndex < 0 ) return;
            
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

                tile.Dissolve(false);
            }
            flow.RemoveRange(positionInList + 1, flow.Count - (positionInList + 1));
        }

        private void SaveState(List<Vector2Int>[] from, List<Vector2Int>[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                to[i].Clear();
                for (int j = 0; j < from[i].Count; j++)
                {
                    to[i].Add(new Vector2Int(from[i][j].x, from[i][j].y));
                }
            }
        }
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

            return false;
        }

        private bool IsMovement()
        {
            return _currentColor != _lastColor;
        }

        private bool AddFlow(Vector2Int lastTile)
        {
            return AddFlow(_currentColor, lastTile);
        }
        private bool AddFlow(int colorIndex,Vector2Int lastTile)
        {

            List<Vector2Int> flow = _currentState[colorIndex];

            if (flow.Contains(lastTile)) return false;

            // Si no esta en la lista, lo anadimos y le ponemos el color
            flow.Add(lastTile);

            Tile tile = _tiles[lastTile.y, lastTile.x];
            tile.SetColor(colorIndex);

            return true;
        }
        private void CrossFlow(Vector2Int position, Vector2Int direction)
        {
            Tile tile = _tiles[position.y, position.x];
            tile.SetFlowActive(direction, true);
            
            tile = _tiles[position.y - direction.y, position.x - direction.x];
            tile.SetFlowActive(-direction, true);
        }

        private void RestoreFlow(Vector2Int tilePosition)
        {
            int i = 0;

            while(i < _intermediateState.Length)
            {
                if (_intermediateState[i].Contains(tilePosition)) break;
                i++;
            }
            
            if (i >= _intermediateState.Length) return;

            // If it is not connected to the actual flow, nothing to do.
            int lastTile = _currentState[i].Count - 1;
            int currentTile = _intermediateState[i].IndexOf(tilePosition);

            if (currentTile - lastTile > 1) return;
            currentTile--;

            while (currentTile < _intermediateState[i].Count - 1)  
            {
                Vector2Int pos = _intermediateState[i][currentTile];
                Tile tile = _tiles[pos.y, pos.x];
                Vector2Int nextPos = _intermediateState[i][currentTile + 1];
                Vector2Int dir = pos - nextPos;

                if (!_currentState[_currentColor].Contains(pos))
                {
                    tile.SetColor(i);
                    AddFlow(i, pos);

                    // Si la siguiente tile del estado actual es del color actual, no te hago cross
                    if (!_currentState[_currentColor].Contains(nextPos)) CrossFlow(pos, dir);
                    else return;
                }
                else return;

                currentTile++;
            }
            AddFlow(i, _intermediateState[i][currentTile]);
        }
        private bool ValidMovement(Vector2Int pos)
        {
            // If it is a gap, the player cannot move there.
            if (_tiles[pos.y, pos.x].IsGap()) return false;
            
            // If there is more than one tile of distance between them, the movement is not valid
            if ((_lastTile - pos).magnitude > 1) return false;

            int flowIndex = _tiles[_lastTile.y, _lastTile.x].GetColorIndex();

            // If there is a circle of another color in the actual tile, the flow cannot grow.
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

        private void SetStarsActive(int colorIndex, bool active)
        {
            Vector2Int starTile = _solution[colorIndex][0];
            _tiles[starTile.y, starTile.x].SetStarActive(active);

            starTile = _solution[colorIndex][_solution[colorIndex].Count - 1];
            _tiles[starTile.y, starTile.x].SetStarActive(active);
        }

        private void UpdatePercentage()
        {
            float tilesFlowing = 0;
            for (int i = 0; i < _currentState.Length; i++) tilesFlowing += _currentState[i].Count;
            
            _levelManager.UpdatePipePercentage((int)(tilesFlowing * 100 / _usableTiles));
        }

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
    }
    
}
