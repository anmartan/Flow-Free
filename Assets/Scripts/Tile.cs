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


        /// <summary>
        /// Makes the tile have a circle, with the specified color.
        /// </summary>
        /// <param name="color">Color of the circle. This cannot be changed afterwards.</param>
        public void PutCircle(Color color)
        {
            // Check so that a circle is not changed after it's been set
            if(!circle.enabled)
            {
                circle.enabled = true;
                circle.color = color;
            }
        }


        public void PutFlow(Color color, Vector2Int direction)
        {
            if(circle.enabled)
            {
                setFlow(exitFlow, color, direction);
            }

            else
            {
                // If it's the first time the tile is used, the entranceFlow is used.
                // Otherwise, the exitFlow is used.
                if (!entranceFlow.enabled)  setFlow(entranceFlow, color, direction);
                else                        setFlow(exitFlow, color, direction);
            }
        }



        public bool hasCircle() { return circle.enabled; }


        public void clearFlow(bool onlyExit)
        {
            clearWay(exitFlow);
            if (!onlyExit) clearWay(entranceFlow);
        }


        private void setFlow(SpriteRenderer flow, Color color, Vector2Int direction)
        {
            flow.enabled = true;
            flow.color = color;
            flow.transform.rotation = Quaternion.identity;
            int rotation = (direction.x == -1) ? 0 : (direction.x == 1) ? 180 : direction.y * -90;

            flow.transform.Rotate(new Vector3(0, 0, rotation));
        }

        private void clearWay(SpriteRenderer sprite)
        {
            sprite.transform.rotation = Quaternion.identity;
            sprite.enabled = false;
        }

    }
}