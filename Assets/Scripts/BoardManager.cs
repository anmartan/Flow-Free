using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class BoardManager : MonoBehaviour
    {
        [Tooltip("Camera in the scene, used to scale the tiles of the grid.")]
        [SerializeField] private Camera sceneCamera;        // Camera of the scene. Used for scaling the grid.

        [Tooltip("Tile prefab. The grid will be filled with these.")]
        [SerializeField] private Tile tilePrefab;           // Prefab of the tile that will be instantiated for creating the grid.

        [Tooltip("Margin at the top of the screen, used for the HUD, in pixels.")]
        [SerializeField] private GameObject topMargin;      // Margins left at both the top and bottom of the screen, for the HUD.

        [Tooltip("Margin at the bottom of the screen screen left unused by the board, in pixels.")]
        [SerializeField] private GameObject bottomMargin;   // Margins left at both sides of the screen, so that the board doesn't occupy the whole screen.

        private int width, height;                          // width and height of the board (in tiles).

        private Tile[,] tiles;                              // tiles array so that they can be accessed later on.

        private List<Vector2Int>[] solution;

        private bool changed;
        private bool touching;
        private Vector2Int lastTile;
        private int currentFlowColor;
        private int colorDelMedio;
        private int lastFlowColor;

        private List<Vector2Int>[] changes;
        private List<Vector2Int>[] currentState;
        private List<Vector2Int>[] lastState;

        private int playerMovements;                        // The number of movements the player has used to solve the level. It changes in two situations:
                                                            // When the player touches a flow (or a circle), different from the last one they touched (playerMovements++).
                                                            // When the player undoes the last movement (playerMovements--).

        /// <summary>
        /// Creates a board the player can interact with. Creates the tiles and sets the initial circles, with the corresponding colors.
        /// Receives a map which has been previously loaded and includes the relevant information for the level.
        /// </summary>
        /// <param name="map">A map with the level information.</param>
        public void CreateBoard(Map map)
        {
            width = map.getWidth();
            height = map.getHeight();
            tiles = new Tile[height, width];

            // Creates the lists that will containt information about the board: the final solution, the current state and the previous state.
            solution = new List<Vector2Int>[map.getFlowsNumber()];
            currentState = new List<Vector2Int>[map.getFlowsNumber()];
            lastState = new List<Vector2Int>[map.getFlowsNumber()];
            changes = new List<Vector2Int>[map.getFlowsNumber()];

            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                solution[i] = new List<Vector2Int>();
                currentState[i] = new List<Vector2Int>();
                lastState[i] = new List<Vector2Int>();
                changes[i] = new List<Vector2Int>();
            }

            // Destroys all of the tiles it saved previously.
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

            // Resets the scale and position, so that when the new pool of objects is created, it is well scaled. 
            transform.localScale = Vector3.one;
            transform.position = Vector3.zero;

            // Instantiates the grid, creating every tile.
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Creates a new tile, and assigns it to the tile in that position in the array.
                    // Instantiates it with a (0.5f, -0.5f) offset, so that the parent is in the top-left corner.
                    tiles[i, j] = Instantiate(tilePrefab, new Vector3(j + 0.5f, -i - 0.5f, 0), Quaternion.identity, transform);
                    tiles[i, j].gameObject.name = "(" + j + ", " + i + ")";
                }
            }

            // Sets the colors for each circle. Gets the color according to the theme,
            // And paints the circles at both the beginning and the end of the flow with such color.
            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                solution[i] = map.getFlows()[i];

                tiles[solution[i][0].y, solution[i][0].x].PutCircle(i);
                tiles[solution[i][solution[i].Count - 1].y, solution[i][solution[i].Count - 1].x].PutCircle(i);
            }

            // Puts the board in the center, visually.
            transform.Translate(new Vector3(-width * 0.5f, height * 0.5f));

            // Scales the camera so that the whole content fits in.
            int size = (width >= height) ? width : height;
            sceneCamera.orthographicSize = size + topMargin.transform.lossyScale.y + bottomMargin.transform.lossyScale.y;
        }

        private void Update()
        {
            // If there is no input, there is nothing to do.
            if (Input.touchCount < 1) return;

            // Gets the position of the touch, in world units.
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = sceneCamera.ScreenToWorldPoint(touch.position);

            // If the touch is not inside the grid boundaries, it is the same as if there was no touch.
            if (!(touchPosition.x > transform.position.x && touchPosition.x < transform.position.x + width &&     // x axis
                touchPosition.y < transform.position.y && touchPosition.y > transform.position.y - height))       // y axis
                return;

            // Calculates which tile the touch corresponds to.
            Vector2Int pos = GetTileCoordinates(touchPosition);
            Tile tile = tiles[pos.y, pos.x];

            // If the touch has just started, take this into consideration:
            // If an empty tile is touched, nothing happens.
            // If a tile is touched and there was a flow coming out from that tile, the rest of the flow has to be dissolved.
            if (touch.phase == TouchPhase.Began)
            {
                int color = tile.GetColorIndex();
                if (color == -1) return;

                //changes = new List<Tile>();
                // RemoveBackground()

                if (tile.IsCircle()) DissolveFlow(color);
                else if (currentState[color].Count > 0) DissolveFlow(color, pos);

                currentFlowColor = color;

                // Add this flow to the list of changes
                AddFlow(pos);

                lastTile = pos;
                touching = true;
            }

            // If the touch is moving, take this into consideration:
            // If it is not a valid movement (for any reason), nothing happens.
            // If the flow cuts a part of itself, the flow has to be correctly dissolved.
            // If the flow cuts another flow, it has to be correctly dissolved.
            // The previous state cannot be destroyed, in case the player wants to undo their last movement.
            else if (touch.phase == TouchPhase.Moved && touching && pos != lastTile)
            {
                if (!ValidMovement(pos)) return;
                Vector2Int direction = pos - lastTile;

                // If there is a flow prior to this movement, some part of the flow has to be dissolved.
                int previousColor = tile.GetColorIndex();
                if (previousColor != -1)
                {
                    int positionInList = currentState[previousColor].IndexOf(pos);

                    // If the flow is the same color, and it is not the missing circle it is removed until this position.
                    if (previousColor == currentFlowColor && !tile.IsCircle() || pos == currentState[currentFlowColor][0]) DissolveFlow(previousColor, currentState[previousColor][positionInList]);
                    
                    // If it is another color, though, it has to be dissolved until the previous position, so that both flows do not share this tile.
                    else if (previousColor != currentFlowColor) DissolveFlow(previousColor, currentState[previousColor][positionInList - 1]);
                }

                if(AddFlow(pos))    CrossFlow(pos, direction);
                lastTile= pos;
            }

            // If the touch has finished (either because it was cancelled or becuase the player lifted their finger, take this into consideration:
            // If there was any change in the flows, the number of movements has to increase.
            // If there was any flow movement, the tiles have to change their background.
            else if (touching && (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended))
            {
                if (changed)
                {
                    SaveState(changes, lastState);
                    lastFlowColor = colorDelMedio;
                    changed = false;
                }
                if (StateChanged())
                {
                    SaveState(currentState, changes);
                    colorDelMedio = currentFlowColor;
                    changed = true;

                    if (IsMovement())
                    {
                        playerMovements++;
                    }
                }
                touching = false;

                Debug.Log(playerMovements);
            }
        }

        /// <summary>
        /// Returns the row and column of the tile that is in touchPosition, in world units.
        /// </summary>
        /// <param name="position">The position of the tile, in world units.</param>
        /// <returns>The position of the tile, in the array of tiles. (x, y)</returns>
        private Vector2Int GetTileCoordinates(Vector3 position)
        {
            // Removes the offset, so that the touch is in the range [(0,0), (width, height)]
            position = position - transform.position;

            // Calculates the tile clicked, using the integer part of each component of the vector
            int row = Mathf.Clamp(Mathf.FloorToInt(-position.y), 0, width - 1);
            int column = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, height - 1);

            return new Vector2Int(column, row);
        }

        private void DissolveFlow(int colorIndex)
        {
            DissolveFlow(colorIndex, new Vector2Int(-1, -1));
        }
        private void DissolveFlow(int colorIndex, Vector2Int lastIncluded)
        {
            List<Vector2Int> flow = currentState[colorIndex];
            int positionInList = flow.IndexOf(lastIncluded);

            // If the touched tile is in the list, the whole list is not removed; only the part that came after touching that tile.
            if (positionInList != -1) tiles[lastIncluded.y, lastIncluded.x].Dissolve(true);

            // Dissolves the rest of the tiles from this point on, and stores them in the list of changes
            for (int i = positionInList + 1; i < flow.Count; i++)
            {
                Vector2Int pos = flow[i];
                Tile tile = tiles[pos.y, pos.x];

                tile.Dissolve(false);
                if (!tile.IsCircle()) tile.SetColor(-1);
                //changes.Add(tile);
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
        private bool StateChanged()
        {
            // If the flow only has one element, it is considered to be unchanged (as it has no effect in the game).
            if (currentState[currentFlowColor].Count <= 1 && lastState[currentFlowColor].Count <= 1) return false;

            // If the current flow and the previous one have different sizes, there has been some changes.
            if (currentState[currentFlowColor].Count != lastState[currentFlowColor].Count) return true;

            // If the flow changed the start, there may have been a change.
            int startIndex = lastState[currentFlowColor].IndexOf(currentState[currentFlowColor][0]);

            // If the previous flow does not contain the new one's start, the flow changed.
            if (startIndex == -1) return true;

            // If it is included, it needs to be in the same order (or inversed).
            int size = currentState[currentFlowColor].Count;
            if (startIndex == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (currentState[currentFlowColor][i] != lastState[currentFlowColor][i]) return true;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    if (currentState[currentFlowColor][startIndex - i] != lastState[currentFlowColor][i]) return true;
                }
            }

            return false;
        }

        private bool IsMovement()
        {
            return currentFlowColor != lastFlowColor;
        }

        private bool AddFlow(Vector2Int lastTile)
        {
            List<Vector2Int> flow = currentState[currentFlowColor];

            if (flow.Contains(lastTile)) return false;

            // Si no esta en la lista, lo anadimos y le ponemos el color
            flow.Add(lastTile);

            Tile tile = tiles[lastTile.y, lastTile.x];
            tile.SetColor(currentFlowColor);
            //changes.Add(tile);
            return true;
        }

        private void CrossFlow(Vector2Int position, Vector2Int direction)
        {
            Tile tile = tiles[position.y, position.x];
            tile.SetFlowActive(direction, true);
            
            tile = tiles[position.y - direction.y, position.x - direction.x];
            tile.SetFlowActive(-direction, true);
        }

        public void UndoMovement()
        {
            foreach (var list in currentState)
            {
                foreach (var tile in list)
                {
                    tiles[tile.y, tile.x].ResetState();
                }
            }
            if (currentFlowColor != lastFlowColor) playerMovements--;
            SaveState(lastState, currentState);
            if (changed) SaveState(lastState, changes);

            for (int i = 0; i < currentState.Length; i++) 
            {
                for (int j = 0; j < currentState[i].Count; j++)
                {
                    Vector2Int pos = currentState[i][j];
                    Tile tile = tiles[pos.y, pos.x];
                    tile.SetColor(i);
                    if (j < currentState[i].Count - 1)
                    {
                        Vector2Int nextPos = currentState[i][j + 1];
                        Vector2Int dir = pos - nextPos;
                        CrossFlow(pos, dir);
                    }
                }
            }
            changed = false;

            Debug.Log(playerMovements);
        }

        private bool ValidMovement(Vector2Int pos)
        {
            // If there is more than one tile of distance between them, the movement is not valid
            if ((lastTile- pos).magnitude > 1) return false;

            int flowIndex = tiles[lastTile.y, lastTile.x].GetColorIndex();

            // If there is a circle of another color in the actual tile, the flow cannot grow.
            if (tiles[pos.y, pos.x].IsCircle() && flowIndex != tiles[pos.y, pos.x].GetColorIndex()) return false;

            // If the flow has both circles covered, and the movement is not used to undo the flow, it is not a valid movement.
            if (currentState[flowIndex].Contains(solution[flowIndex][0]))// &&                              // Contains the first circle
               if(currentState[flowIndex].Contains(solution[flowIndex][solution[flowIndex].Count - 1]))// &&   // Contains the second circle
                if(!currentState[flowIndex].Contains(pos))                                                   // Is not an "undo" movement
                    return false;

            // If everything else was correct, the movement is a valid one.
            return true;
        }
    }
    
}
