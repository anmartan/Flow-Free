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

        [Tooltip("Margin at both the top and bottom of the screen, used for the HUD, in pixels.")]
        [SerializeField] private float verticalMargin;

        [Tooltip("Margin at each sides of the screen screen left unused by the board, in pixels.")]
        [SerializeField] private float sideMargin;


        private Tile[,] tiles;                 // tiles array so that they can be accessed later on.

        private float scale;
        private int width, height;
        public void createBoard(Map map)
        {
            // [TODO] getters del mapa
            width = map.width;
            height = map.height;
            tiles = new Tile[height, width];

            // Destroys all of the tiles it saved previously. This should always be empty but,
            // Just in case, it's emptied before being used.
            for (int i = 0; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
            
            // Resets the scale, so that when the new pool of objects is created, it is well scaled. 
            // Just in case, as this method should be called only once when the scene is created.
            transform.localScale = Vector3.one;

            // Instantiates the grid, creating every tile
            for (int i= 0; i < height; i++)
            {
                for(int j = 0; j < width; j++)
                {
                    // Creates a new tile, and assigns it to the tile in that position in the array
                    // Instantiates it with a (0.5f, -0.5f) offset, so that the parent is in the top-left corner.
                    tiles[i, j] = Instantiate(tile, new Vector3(j + 0.5f, -i - 0.5f, 0), Quaternion.identity, transform).GetComponent<Tile>();
                }
            }

            // [TODO] no se ni que decir
            for (int i = 0; i < map.flowsNumber; i++) 
            {
                Color color = GameManager.Instance().getActualTheme().colors[i];
                tiles[(int)map.flows[i][0].x, (int)map.flows[i][0].y].PutCircle(color);
                tiles[(int)map.flows[i][map.flows[i].Count - 1].x, (int)map.flows[i][map.flows[i].Count - 1].y].PutCircle(color);
            }


            // Calculate the size of the grid, given the height and width of the screen.
            scale = camera.orthographicSize * 2;
            if (width >= height) scale = (scale * (Screen.width - 2 * sideMargin) / (Screen.height)) / width;

            // [TODO] fix
            else
            {
                // If it's scaling using the height of the board, the top and bottom margins are substracted from the available space.
                Vector3 margins = camera.ScreenToWorldPoint(new Vector3(verticalMargin, 0));
                scale -= (margins.x * 2);

                scale = (scale * Screen.width / Screen.height) / height;
            }

            // Scale the grid accordingly.
            transform.Translate(new Vector3(-width * 0.5f * scale, height * 0.5f * scale));
            transform.localScale = new Vector3(scale, scale);
        }

        private void Update()
        {
            if (Input.touchCount > 0) 
            {
                Touch touch = Input.GetTouch(0);
                if(touch.phase == TouchPhase.Began)
                {
                    Vector3 touchPosition = camera.ScreenToWorldPoint(touch.position);
                    Vector2 tile;
                    // Checks that the touch is within the grid boundaries
                    if (touchPosition.x > transform.position.x && touchPosition.x < transform.position.x + (scale * width) &&
                        touchPosition.y < transform.position.y && touchPosition.y > transform.position.y - (scale * height))
                    {
                        // Removes the offset, so that the touch is in the range [(0,0), (width * scale, height * scale)]
                        touchPosition = touchPosition - transform.position;

                        // Transforms the vector using the scale, so that the touch is in the range [(0,0), (width, height)]
                        touchPosition /= scale;

                        // Calculates the tile clicked, using the integer part of each component of the vector
                        int row = Mathf.FloorToInt(-touchPosition.y);
                        int column = Mathf.FloorToInt(touchPosition.x);


                        tiles[row, column].PutCircle(new Color(0.15f * row, 0.2f * column, 0.35f * (row + column)));

                    }
                }
            }
        }
    }
}
