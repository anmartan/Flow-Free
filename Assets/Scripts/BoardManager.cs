using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class BoardManager : MonoBehaviour
    {
        [Tooltip("Camera in the scene, used to scale the tiles of the grid.")]
        [SerializeField] private Camera camera;     // camera of the scene. Used for scaling the grid.
        
        [Tooltip("Tile prefab. The grid will be filled with these.")]
        [SerializeField] private GameObject tile;   // prefab of the tile that will be instantiated for creating the grid.

        // [TODO]
        // Is this better than using the boardManager as the pool itself?
        //[Tooltip("GameObject that will store the tiles.")]
        //[SerializeField] private GameObject grid;  // Grid that will save the tiles.

        // Coordinates of the first tile (center) of the board. Used to check the input.
        float initialX, initialY;

        public void createBoard(int width, int height)
        {
            // Destroys all of the tiles it saved previously. This should always be empty but,
            // Just in case, it's emptied before being used.
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);

            // Calculates where the upper left tile will be placed
            initialX = -(width - tile.transform.lossyScale.x) * 0.5f;
            initialY = (height - tile.transform.lossyScale.y) * 0.5f;

            // Instantiates the grid, creating every tile
            for (int i= 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    // Calculate the position of the tile, so that the final result is centered
                    float x = j + initialX;
                    float y = -i + initialY;
                    GameObject obj = Instantiate(tile, new Vector3(x, y, 0), Quaternion.identity, transform);

                    // [TODO]
                    // erase, only used to check whether the PutCircle works or not
                    if (i == j)
                    {
                        obj.GetComponent<Tile>().PutCircle(new Color(0.15f * i, 0.2f * i, 0.35f * i));
                    }
                }
            }

            // Calculate the size of the grid, given the height and width of the screen.
            float scale = camera.orthographicSize * 2 * Screen.width/ (width * Screen.height);

            // Scale the grid accordingly.
            transform.localScale = new Vector3(scale, scale);
        }

        public void OnMouseDrag()
        {
            // [TODO]
            /*
             * This is where the drags are checked, not in each tile individually. 
             * The board checks which tiles are being dragged, and tells them to change their color.
             * The tile is told to activate its flow, and the direction in which to do so (or, in the case of a corner, the two directions).
             * By doing so, the board can check if it's complete, and if it's correct. 
             * 
             * The Tile component has a OnMouseDrag method, commented, to check how it worked. It should be removed later.
             */
        }
    }
}
