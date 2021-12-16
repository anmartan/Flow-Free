using UnityEngine;


namespace FlowFree
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("Sprite used for rendering the background of the tile.")]
        [SerializeField] private SpriteRenderer background;
        
        [Tooltip("Children of the tile, used for rendering one of the directions the flow takes.")]
        [SerializeField] private SpriteRenderer entranceFlow;

        [Tooltip("Child of the tile, used for rendering the other direction the flow takes.")]
        [SerializeField] private SpriteRenderer exitFlow;

        [Tooltip("Sprite used for drawing the circles in the needed tiles.")]
        [SerializeField] private SpriteRenderer circle;

        [Tooltip("Sprite used on top of the circles when a hint is given.")]
        [SerializeField] private SpriteRenderer star;

        [Tooltip("Sprite used for drawing the north wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer northWall;
        
        [Tooltip("Sprite used for drawing the south wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer southWall;
        
        [Tooltip("Sprite used for drawing the east wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer eastWall;
        
        [Tooltip("Sprite used for drawing the west wall of the tile, if there is one.")]
        [SerializeField] private SpriteRenderer westWall;
        
        private int connections = 0;
        private int colorIndex = -1;
        private Color color = Color.black;
        private bool _gap;

        public void PutCircle(int index)
        {
            SetColor(index);
            circle.enabled = true;
            circle.color = color;
            connections++;
        }

        public void SetStarActive(bool active)
        {
            if (circle.enabled) star.enabled = active;
        }

        public void SetWallActive(Vector2Int direction)
        {
            if (direction == Vector2Int.up) northWall.enabled = true;
            else if (direction == Vector2Int.down) southWall.enabled = true;
            else if (direction == Vector2Int.right) eastWall.enabled = true;
            else westWall.enabled = true;
        }

        public void SetActive(bool active)
        {
            background.enabled = active;
            _gap = !active;
        }
        public bool IsWallActive(Vector2Int direction)
        {
            if (direction == Vector2Int.up) return northWall.enabled;
            if (direction == Vector2Int.down) return southWall.enabled;
            if (direction == Vector2Int.right) return eastWall.enabled;
            return westWall.enabled;
        }
        public void SetColor(int index)
        {
            colorIndex = index;
            if (index < 0) return;

            color = GameManager.Instance().getActualTheme().colors[index];
            circle.color = color;
            entranceFlow.color = color;
            exitFlow.color = color;
        }
        public void SetFlowActive(Vector2Int direction, bool active)
        {
            SpriteRenderer aux;

            if (connections < 1) aux = entranceFlow;
            else aux = exitFlow;

            if (active && !aux.enabled) connections++;
            else if(!active && aux.enabled) connections--;

            aux.enabled = active;
            aux.transform.up = new Vector3(direction.x, -direction.y, 0);
            aux.color = color;
        }
        public bool IsGap() { return _gap; }
        public bool IsFullyConnected() { return connections == 2; }
        public int GetConnections() { return connections; }
        public int GetColorIndex() { return colorIndex; }
        public bool IsCircle() { return circle.enabled; }

        public void Dissolve(bool onlyExit)
        {
            ClearWay(exitFlow);
            if (!onlyExit) ClearWay(entranceFlow);

            if (!entranceFlow.enabled && !exitFlow.enabled && !circle.enabled) colorIndex = -1;
        }

        public void ResetState()
        {
            Dissolve(false);
            if(!circle.enabled) colorIndex = -1;
        }
        
        private void ClearWay(SpriteRenderer sprite)
        {
            sprite.transform.rotation = Quaternion.identity;
            if (sprite.enabled) connections--;
            sprite.enabled = false;            
        }
    }
}