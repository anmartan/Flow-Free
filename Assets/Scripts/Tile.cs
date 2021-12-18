using System.IO.Compression;
using UnityEngine;


namespace FlowFree
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("SpriteRenderer used for rendering the background of the tile.")]
        [SerializeField] private SpriteRenderer _background;    // Sprite used for rendering the background of the tile.
        
        [Tooltip("SpriteRenderer used for rendering one of the directions the flow takes.")]
        [SerializeField] private SpriteRenderer _entranceFlow;  // SpriteRenderer used for rendering one of the directions the flow takes.

        [Tooltip("SpriteRenderer used for rendering the other direction the flow takes.")]
        [SerializeField] private SpriteRenderer _exitFlow;      // SpriteRenderer used for rendering the other direction the flow takes.

        [Tooltip("SpriteRenderer used for drawing the circles in the needed tiles.")]
        [SerializeField] private SpriteRenderer _circle;        // SpriteRenderer used for drawing the circles in the needed tiles.

        [Tooltip("SpriteRenderer used on top of the circles when a hint is given.")]
        [SerializeField] private SpriteRenderer _star;          // SpriteRenderer used on top of the circles when a hint is given.

        [Tooltip("SpriteRenderer used for drawing the north wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer _northWall;     // SpriteRenderer used for drawing the north wall of the tile, if there is one.
        
        [Tooltip("SpriteRenderer used for drawing the south wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer _southWall;     // SpriteRenderer used for drawing the south wall of the tile, if there is one.
        
        [Tooltip("SpriteRenderer used for drawing the east wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer _eastWall;      // SpriteRenderer used for drawing the east wall of the tile, if there is one.
        
        [Tooltip("SpriteRenderer used for drawing the west wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer _westWall;      // SpriteRenderer used for drawing the west wall of the tile, if there is one.
        
        private int _connections = 0;                           // The number of connections a tile has. If it is a circle, it has one with itself.
        private int _colorIndex = -1;                           // The index of the color the tile has. 
        private Color _color = Color.black;                     // The color that index corresponds to, when using a specific theme.
        private bool _gap;                                      // Whether the tile is usable or not.

        
        // ----- SETTERS ----- //
        
        /// <summary>
        /// Changes the visibility of the tile, and therefore its ability to be interacted with.
        /// </summary>
        /// <param name="active">Whether the tile will be active or not.</param>
        public void SetActive(bool active)
        {
            _background.enabled = active;
            _gap = !active;                 // If the tile is active, it cannot be a gap, and vice versa.
        }
        
        /// <summary>
        /// Puts a circle in the tile, if there wasn't one previously.
        /// </summary>
        /// <param name="index">Index of the color the circle will have.</param>
        public void SetCircle(int index)
        {
            // Sets the color for the circle.
            SetColor(index);
            
            // Enables the circle, and adds one to the connections.
            _circle.enabled = true;
            _connections++;
        }

        /// <summary>
        /// Sets the color for all the elements in this tile.
        /// </summary>
        /// <param name="index">Index of the color in the theme.</param>
        public void SetColor(int index)
        {
            _colorIndex = index;
            
            // If the index is not a valid one (-1, when the index is used to detect errors), no color is changed.
            if (index < 0 || index >= GameManager.Instance().getActualTheme().colors.Length) return;

            // Sets the color for all the elements.
            _color = GameManager.Instance().getActualTheme().colors[index];
            _circle.color = _color;
            _entranceFlow.color = _color;
            _exitFlow.color = _color;
        }
        
        /// <summary>
        /// Changes the visibility of the flow, in the given direction.
        /// </summary>
        /// <param name="direction">The direction of the flow that will be changed.</param>
        /// <param name="active">Whether the flow is active or not in such direction.</param>
        public void SetFlowActive(Vector2Int direction, bool active)
        {
            SpriteRenderer aux;

            if (_connections < 1) aux = _entranceFlow;
            else aux = _exitFlow;

            if (active && !aux.enabled) _connections++;
            else if(!active && aux.enabled) _connections--;

            aux.enabled = active;
            aux.transform.up = new Vector3(direction.x, -direction.y, 0);
            aux.color = _color;
        }
        
        /// <summary>
        /// Changes the visibility of the star, if the tile is a circle.
        /// </summary>
        /// <param name="active">Whether the star will be visible or not.</param>
        public void SetStarActive(bool active)
        {
            if (_circle.enabled) _star.enabled = active;
        }
        
        /// <summary>
        /// Puts a wall in the direction given.
        /// </summary>
        /// <param name="direction"> Direction in which the wall will be set.</param>
        public void SetWallActive(Vector2Int direction)
        {
            if (direction == Vector2Int.up) _northWall.enabled = true;
            else if (direction == Vector2Int.down) _southWall.enabled = true;
            else if (direction == Vector2Int.right) _eastWall.enabled = true;
            else _westWall.enabled = true;
        }
        
        // ----- GETTERS ----- //
        
        // TODO: Comment and organize
        public bool IsWallActive(Vector2Int direction)
        {
            if (direction == Vector2Int.up) return _northWall.enabled;
            if (direction == Vector2Int.down) return _southWall.enabled;
            if (direction == Vector2Int.right) return _eastWall.enabled;
            return _westWall.enabled;
        }
        
        public bool IsGap() { return _gap; }
        public bool IsFullyConnected() { return _connections == 2; }
        public int GetColorIndex() { return _colorIndex; }
        public bool IsCircle() { return _circle.enabled; }

        public void Dissolve(bool onlyExit)
        {
            ClearWay(_exitFlow);
            if (!onlyExit) ClearWay(_entranceFlow);

            if (!_entranceFlow.enabled && !_exitFlow.enabled && !_circle.enabled) _colorIndex = -1;
        }
        
        private void ClearWay(SpriteRenderer sprite)
        {
            sprite.transform.rotation = Quaternion.identity;
            if (sprite.enabled) _connections--;
            sprite.enabled = false;            
        }
    }
}