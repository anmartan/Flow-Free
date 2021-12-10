using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class BoardManager : MonoBehaviour
    {
        [Tooltip("Camera in the scene, used to scale the tiles of the grid.")]
        [SerializeField] private Camera sceneCamera;     // Camera of the scene. Used for scaling the grid.

        [Tooltip("Tile prefab. The grid will be filled with these.")]
        [SerializeField] private Tile tile;             // Prefab of the tile that will be instantiated for creating the grid.

        [Tooltip("Margin at both the top and bottom of the screen, used for the HUD, in pixels.")]
        [SerializeField] private float verticalMargin;  // Margins left at both the top and bottom of the screen, for the HUD.

        [Tooltip("Margin at each sides of the screen screen left unused by the board, in pixels.")]
        [SerializeField] private float sideMargin;      // Margins left at both sides of the screen, so that the board doesn't occupy the whole screen.

        private float scale;                            // scale of the board. Calculated when the board is created.
        private int width, height;                      // width and height of the board (in tiles).

        private Tile[,] tiles;                          // tiles array so that they can be accessed later on.
        private int[,] logicTiles;                      // logic representation of the board; -1 means the tile is not occupied, values in the range [0, map.flowsNumber - 1] represent one of the flows.

        private Vector2Int lastTileclicked;
        private int lastFlowClicked;
        private bool touching;

        private List<Vector2Int>[] solution;            // The final solution to the puzzle.
        private List<Vector2Int>[] currentFlowsList;    // The state of the flows, meaning the connected tiles depending on the level.
        private List<Vector2Int>[] previousFlowsList;   // The state before the last player movement, so that the player can undo the last movement.
                                                        // Also used when the player breaks one of the existing flows, crossing it with another one.

        private bool boardChanged;                      // true if the board has changed with the last interaction; the state must be saved so it can be recovered later.
        private int playerMovements;                    // The number of movements the player has used to solve the level. It changes in two situations:
                                                        // When the player touches a flow (or a circle), different from the last one they touched (playerMovements++).
                                                        // When the player undoes the last movement (playerMovements--).

        /// <summary>
        /// Creates a board the player can interact with. Creates the tiles and sets the initial circles, with the corresponding colors.
        /// Receives a map which has been previously loaded and includes the relevant information for the level.
        /// </summary>
        /// <param name="map">A map with the level information.</param>
        public void createBoard(Map map)
        {
            width = map.getWidth();
            height = map.getHeight();
            tiles = new Tile[height, width];
            logicTiles = new int[height, width];

            // Creates the lists that will containt information about the board: the final solution, the current state and the previous state.
            solution = new List<Vector2Int>[map.getFlowsNumber()];
            currentFlowsList = new List<Vector2Int>[map.getFlowsNumber()];
            previousFlowsList = new List<Vector2Int>[map.getFlowsNumber()];

            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                solution[i] = new List<Vector2Int>();
                currentFlowsList[i] = new List<Vector2Int>();
                previousFlowsList[i] = new List<Vector2Int>();
            }

            // Destroys all of the tiles it saved previously. This should always be empty but,
            // Just in case, it's emptied before being used.
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

            // Resets the scale, so that when the new pool of objects is created, it is well scaled. 
            // Just in case, as this method should be called only once when the scene is created.
            transform.localScale = Vector3.one;

            // Instantiates the grid, creating every tile
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // Creates a new tile, and assigns it to the tile in that position in the array
                    // Instantiates it with a (0.5f, -0.5f) offset, so that the parent is in the top-left corner.
                    tiles[i, j] = Instantiate(tile, new Vector3(j + 0.5f, -i - 0.5f, 0), Quaternion.identity, transform);
                    logicTiles[i, j] = -1;
                }
            }

            // Sets the colors for each circle. Gets the color according to the theme,
            // And paints the circles at both the beginning and the end of the flow with such color.
            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                Color color = GameManager.Instance().getActualTheme().colors[i];

                solution[i] = map.getFlows()[i];

                tiles[solution[i][0].x, solution[i][0].y].PutCircle(color);
                tiles[solution[i][solution[i].Count - 1].x, solution[i][solution[i].Count - 1].y].PutCircle(color);

                // Updates the tilesIndices value
                logicTiles[solution[i][0].x, solution[i][0].y] = i;
                logicTiles[solution[i][solution[i].Count - 1].x, solution[i][solution[i].Count - 1].y] = i;
            }


            // Calculate the size of the grid, given the height and width of the screen.
            scale = sceneCamera.orthographicSize * 2;
            if (width >= height) scale = (scale * (Screen.width - 2 * sideMargin) / (Screen.height)) / width;

            // [TODO] fix
            //else
            //{
            //    // If it's scaling using the height of the board, the top and bottom margins are substracted from the available space.
            //    Vector3 margins = sceneCamera.ScreenToWorldPoint(new Vector3(verticalMargin, 0));
            //    scale -= (margins.x * 2);

            //    scale = (scale * Screen.width / Screen.height) / height;
            //}

            // Scale the grid accordingly.
            transform.Translate(new Vector3(-width * 0.5f * scale, height * 0.5f * scale));
            transform.localScale = new Vector3(scale, scale);

        }

        private void Update()
        {
            // If there is no input, there is nothing to do.
            if (Input.touchCount < 1) return;

            // Gets the position of the touch, in world units.
            Touch touch = Input.GetTouch(0);
            Vector3 touchPosition = sceneCamera.ScreenToWorldPoint(touch.position);

            // If the touch is not inside the grid boundaries, it is the same as if there was no touch.
            if (!(touchPosition.x > transform.position.x && touchPosition.x < transform.position.x + (scale * width) &&     // x axis
                touchPosition.y < transform.position.y && touchPosition.y > transform.position.y - (scale * height)))       // y axis
            {
                touching = false;
                return;
            }

            // Calculates which tile the touch corresponds to.
            Vector2Int pos = getTile(touchPosition);

            // If the touch has just started, take this into consideration:
            // If an empty tile is touched, nothing happens.
            // If a tile is touched and there was a flow coming out from that tile, the rest of the flow has to be dissolved.
            if (touch.phase == TouchPhase.Began)
            {
                // What flow is being touched? If it is -1, it is not a valid input
                int flowIndex = logicTiles[pos.x, pos.y];
                if (flowIndex == -1) return;

                lastFlowClicked = flowIndex;
                // Removes the background color until the touch has finished
                RemoveBackground();

                // Checks if there was another flow with that color and dissolves it
                if (currentFlowsList[flowIndex].Count > 0 || tiles[pos.x, pos.y].isCircle())
                {
                    saveState();
                    dissolveFlow(flowIndex, pos);
                }
                addFlow(pos);

                // Updates this values for later checks
                lastTileclicked = pos;
                touching = true;
            }

            // If the touch is moving, and it comes from a valid point in the grid, take this into consideration:
            // If it is not a valid movement (for any reason), nothing happens.
            // If the flow cuts a part of itself, the flow has to be correctly dissolved.
            // If the flow cuts another flow, it has to be correctly dissolved.
            // The previous state cannot be destroyed, in case the player wants to undo their last movement.
            else if (touch.phase == TouchPhase.Moved && touching && pos != lastTileclicked)
            {
                // If this is not a valid movement, there is nothing to do.
                if (!validMovement(pos)) return;

                // If it is the first change, the state is saved so that it can be recovered later on, if the player wants to undo a movement
                if (!boardChanged) saveState();

                // If there is a flow prior to this movement, some part of the flow has to be dissolved.
                int previousFlowIndex = logicTiles[pos.x, pos.y];
                if (previousFlowIndex != -1 && !tiles[pos.x, pos.y].isCircle())
                {
                    // If the flow is the same color, it is removed until this position.
                    int posInList = currentFlowsList[previousFlowIndex].IndexOf(pos);

                    // If it is another color, though, it has to be dissolved until the previous position, so that both flows do not share this tile.
                    if (previousFlowIndex != lastFlowClicked) posInList--;
                    dissolveFlow(previousFlowIndex, currentFlowsList[previousFlowIndex][posInList]);

                    if (previousFlowIndex != lastFlowClicked) updateFlow(pos);
                }
                else updateFlow(pos);
                lastTileclicked = pos;
            }

            // If the touch has finished (either because it was cancelled or becuased the player lifted their finger, take this into consideration:
            // If there was any change in the flows, the number of movements has to increase.
            // If there was any flow movement, the tiles have to change their background.
            else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Ended)
            {
                AddBackground();
                if (StateChanged()) playerMovements++;
                touching = false;
                boardChanged = false;
            }
        }


        /// <summary>
        /// Returns the row and column of the tile that is in touchPosition, in world units.
        /// </summary>
        /// <param name="touchPosition">The position of the tile, in world units.</param>
        /// <returns>The position of the tile, in the array of tiles. (row, column)</returns>
        private Vector2Int getTile(Vector3 touchPosition)
        {
            // Removes the offset, so that the touch is in the range [(0,0), (width * scale, height * scale)]
            touchPosition = touchPosition - transform.position;

            // Transforms the vector using the scale, so that the touch is in the range [(0,0), (width, height)]
            touchPosition /= scale;

            // Calculates the tile clicked, using the integer part of each component of the vector
            int row = Mathf.Clamp(Mathf.FloorToInt(-touchPosition.y), 0, width - 1);
            int column = Mathf.Clamp(Mathf.FloorToInt(touchPosition.x), 0, height - 1);

            return new Vector2Int(row, column);
        }

        private void AddBackground()
        {
            List<Vector2Int> flow = currentFlowsList[lastFlowClicked];
            if(flow.Count > 1)
            {
                for (int i = 0; i < flow.Count; i++)
                    tiles[flow[i].x, flow[i].y].setBackgroundColor(GameManager.Instance().getActualTheme().colors[lastFlowClicked]);
            }
        }

        private void RemoveBackground()
        {
            List<Vector2Int> flow = currentFlowsList[lastFlowClicked];

            for (int i = 0; i < flow.Count; i++)
                tiles[flow[i].x, flow[i].y].removeBackgroundColor();
        }

        private void saveState()
        {
            for (int i = 0; i < solution.Length; i++) 
            {
                previousFlowsList[i].Clear();
                for(int j = 0; j < currentFlowsList[i].Count; j++)
                {
                    previousFlowsList[i].Add(currentFlowsList[i][j]);
                }
            }
            boardChanged = true;
        }
        
        // Only checks the last flow; it is the only one that can cause changes: if another one has changed, it is because this one has.
        private bool StateChanged()
        {
            // If there was no change, the state has not changed (obviously).
            if (!boardChanged) return false;

            // If the flow only has one element, it is considered to be unchanged (as it has no effect in the game).
            if (currentFlowsList[lastFlowClicked].Count <= 1 && previousFlowsList[lastFlowClicked].Count <= 1) return false;

            // If the current flow and the previous one have different sizes, there has been some changes.
            if (currentFlowsList[lastFlowClicked].Count != previousFlowsList[lastFlowClicked].Count) return true;

            // If the flow changed the start, there may have been a change.
            int startIndex = previousFlowsList[lastFlowClicked].IndexOf(currentFlowsList[lastFlowClicked][0]);

            // If the previous flow does not contain the new one's start, the flow changed.
            if (startIndex == -1) return true;

            // If it is included, it needs to be in the same order (or inversed)
            int size = currentFlowsList[lastFlowClicked].Count;
            if (startIndex == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    if (currentFlowsList[lastFlowClicked][i] != previousFlowsList[lastFlowClicked][i]) return true;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    if (currentFlowsList[lastFlowClicked][size - i] != previousFlowsList[lastFlowClicked][i]) return true;
                }
            }

            return false;
        }

        private void dissolveFlow(int flowIndex, Vector2Int lastTile)
        {
            List<Vector2Int> flow = currentFlowsList[flowIndex];
            int posInList = flow.IndexOf(lastTile);

            // If the touched tile is in the list, the whole list is not removed; only the part that came after touching that tile.
            if(posInList != -1) tiles[lastTile.x, lastTile.y].clearFlow(true);
            
            for (int i = posInList + 1; i < flow.Count; i++)
            {
                Vector2Int pos = flow[i];
                Tile tile = tiles[pos.x, pos.y];
                tile.clearFlow(false);
                if (!tile.isCircle()) logicTiles[pos.x, pos.y] = -1;
            }
            flow.RemoveRange(posInList + 1, flow.Count - (posInList + 1));
            boardChanged = true;
        }
        private void updateFlow(Vector2Int lastTile)
        {
            addFlow(lastTile);

            // Calculates the direction in which the flow should grow.
            Vector2Int direction = lastTile - lastTileclicked;
            drawFlow(lastTile, direction);
        }
        private void addFlow(Vector2Int lastTile)
        {
            List<Vector2Int> flow = currentFlowsList[lastFlowClicked];

            if (flow.Contains(lastTile)) return;

            // Si no esta en la lista, lo anadimos y le ponemos el color
            flow.Add(lastTile);
            logicTiles[lastTile.x, lastTile.y] = lastFlowClicked;
            boardChanged = true;
        }
        private void drawFlow(Vector2Int tilePos, Vector2Int direction)
        {
            Tile tile = tiles[tilePos.x, tilePos.y];
            tile.AddFlow(GameManager.Instance().getActualTheme().colors[lastFlowClicked], direction);

            tile = tiles[tilePos.x - direction.x, tilePos.y - direction.y];
            tile.AddFlow(GameManager.Instance().getActualTheme().colors[lastFlowClicked], -direction);
        }
        private bool validMovement(Vector2Int pos)
        {
            int flowIndex = logicTiles[lastTileclicked.x, lastTileclicked.y];
            // If there is more than one tile of distance between them, the movement is not valid
            if ((lastTileclicked - pos).magnitude > 1) return false;

            // If there is a circle of another color in the actual tile, the flow cannot grow.
            if (tiles[pos.x, pos.y].isCircle() && flowIndex != logicTiles[pos.x, pos.y]) return false;

            // If the flow has both circles covered, and the movement is not used to undo the flow, it is not a valid movement.
            if (currentFlowsList[flowIndex].Contains(solution[flowIndex][0]) &&                              // Contains the first circle
               currentFlowsList[flowIndex].Contains(solution[flowIndex][solution[flowIndex].Count - 1]) &&   // Contains the second circle
               !currentFlowsList[flowIndex].Contains(pos))                                                   // Is not an "undo" movement
                return false;

            // If everything else was correct, the movement is a valid one.
            return true;
        }
    }
    
}
