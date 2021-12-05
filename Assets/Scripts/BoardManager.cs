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
        private bool touching;

        private List<Vector2Int>[] actualFlowsList;     // The state of the flows, meaning the connected tiles depending on the level.
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

                List<Vector2> flow = map.getFlows()[i];
                tiles[(int)flow[0].x, (int)flow[0].y].PutCircle(color);
                tiles[(int)flow[flow.Count - 1].x, (int)flow[flow.Count - 1].y].PutCircle(color);

                // Updates the tilesIndices value
                logicTiles[(int)flow[0].x, (int)flow[0].y] = i;
                logicTiles[(int)flow[flow.Count - 1].x, (int)flow[flow.Count - 1].y] = i;
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

            // The initial state of the game is an empty grid, with no flows drawn
            actualFlowsList = new List<Vector2Int>[map.getFlowsNumber()];
            previousFlowsList = new List<Vector2Int>[map.getFlowsNumber()];

            for (int i = 0; i < map.getFlowsNumber(); i++)
            {
                actualFlowsList[i] = new List<Vector2Int>();
                previousFlowsList[i] = new List<Vector2Int>();
            }
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

            if (touch.phase == TouchPhase.Began)
            {
                // What flow is being touched? If it is -1, it is not a valid input
                int flowIndex = logicTiles[pos.x, pos.y];
                if (flowIndex == -1) return;

                // Checks if there was another flow with that color and dissolves it
                if (actualFlowsList[flowIndex].Count > 0 || tiles[pos.x, pos.y].hasCircle())
                {
                    dissolveFlow(flowIndex, pos);
                }
                addFlow(flowIndex, pos);

                lastTileclicked = pos;
                touching = true;
            }

            else if (touch.phase == TouchPhase.Moved && touching)
            {
                if(pos != lastTileclicked)
                {
                    // Uses the information of the last tile it came from, as it will be added to the same list.
                    int flowIndex = logicTiles[lastTileclicked.x, lastTileclicked.y];

                    // Checks if there is another flow in this position; in that case, the flow below has to be dissolved
                    int previousFlowIndex = logicTiles[pos.x, pos.y];

                    // If there is a circle of another color in the actual tile, the flow cannot grow.
                    if (tiles[pos.x, pos.y].hasCircle() && flowIndex != previousFlowIndex) return;

                    // If there is a flow prior to this movement, some part of the flow has to be dissolved

                    /*
                        if(previousFlowIndex != -1)
                        {
                            // If it is the same color, it has to be dissolved to this very position.
                            // However, if it is a different color, it has to be dissolved until the tile before that one, or there would be two colors in the same tile.
                            int posInList = actualFlowsList[previousFlowIndex].IndexOf(pos);
                            if (previousFlowIndex != flowIndex) posInList--;

                            dissolveFlow(previousFlowIndex, actualFlowsList[previousFlowIndex][posInList]);
                        }
                        else
                        {
                            // Calculates the direction in which the flow should grow.
                            Vector2Int direction = pos - lastTileclicked;

                            // Otherwise, the flow grows.
                            addFlow(flowIndex, pos);
                            drawFlow(flowIndex, pos, direction);
                        }
                    */

                    // Calculates the direction in which the flow should grow.
                    Vector2Int direction = pos - lastTileclicked;
                    addFlow(flowIndex, pos);
                    drawFlow(flowIndex, pos, direction);
                    lastTileclicked = pos;
                }
            }

            else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Ended)
            {
                touching = false;
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

        private void dissolveFlow(int flowIndex, Vector2Int lastTile)
        {
            List<Vector2Int> flow = actualFlowsList[flowIndex];
            int posInList = flow.IndexOf(lastTile);

            // If the touched tile is in the list, the whole list is not removed; only the part that came after touching that tile.
            if(posInList != -1) tiles[lastTile.x, lastTile.y].clearFlow(true);
            
            for (int i = posInList + 1; i < flow.Count; i++)
            {
                Vector2Int pos = flow[i];
                Tile tile = tiles[pos.x, pos.y];
                tile.clearFlow(false);
                if (!tile.hasCircle()) logicTiles[pos.x, pos.y] = -1;
            }
            flow.RemoveRange(posInList + 1, flow.Count - (posInList + 1));
        }

        private void addFlow(int flowIndex, Vector2Int lastTile)
        {
            List<Vector2Int> flow = actualFlowsList[flowIndex];

            if (flow.Contains(lastTile)) return;

            // Si no esta en la lista, lo anadimos y le ponemos el color
            flow.Add(lastTile);
            logicTiles[lastTile.x, lastTile.y] = flowIndex;
        }

        private void drawFlow(int flowIndex, Vector2Int tilePos, Vector2Int direction)
        {
            Tile tile = tiles[tilePos.x, tilePos.y];
            tile.PutFlow(GameManager.Instance().getActualTheme().colors[flowIndex], direction);

            tile = tiles[tilePos.x - direction.x, tilePos.y - direction.y];
            tile.PutFlow(GameManager.Instance().getActualTheme().colors[flowIndex], -direction);
        }
    }
    
}
