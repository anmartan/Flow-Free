using UnityEngine;


namespace FlowFree
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("Children of the tile, used for rendering one of the directions the flow takes.")]
        [SerializeField] private SpriteRenderer entranceFlow;

        [Tooltip("Child of the tile, used for rendering the other direction the flow takes.")]
        [SerializeField] private SpriteRenderer exitFlow;

        [Tooltip("Sprite used for drawing the circles in the needed tiles.")]
        [SerializeField] private SpriteRenderer circle;

        [Tooltip("Sprite used for drawing the background square, which will be a specific color depending on the flow it contains.")]
        [SerializeField] private SpriteRenderer background;

        private int connections = 0;
        private int colorIndex = -1;
        private Color color = Color.black;

        public void PutCircle(int index)
        {
            SetColor(index);
            circle.enabled = true;
            circle.color = color;
            connections++;
        }
        public void SetColor(int index)
        {
            colorIndex = index;
            if(index >= 0)  color = GameManager.Instance().getActualTheme().colors[index];
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

        public int GetConnections() { return connections; }
        public int GetColorIndex() { return colorIndex; }
        public bool IsCircle() { return circle.enabled; }

        public void Dissolve(bool onlyExit)
        {
            clearWay(exitFlow);
            if (!onlyExit) clearWay(entranceFlow);

            if (!entranceFlow.enabled && !exitFlow.enabled && !circle.enabled) colorIndex = -1;
        }

        public void ResetState()
        {
            Dissolve(false);
            if(!circle.enabled) colorIndex = -1;
        }
        
        private void clearWay(SpriteRenderer sprite)
        {
            sprite.transform.rotation = Quaternion.identity;
            if (sprite.enabled) connections--;
            sprite.enabled = false;            
        }
    }
}