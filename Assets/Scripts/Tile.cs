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

        /// <summary>
        /// Puts a flow, in the given direction with the given color.
        /// It activates the entrance flow if it is the first time the tile is being used.
        /// It activates the exit flow if the tile is being used for the second time, or if it has a circle (in which case, there can only be one flow).
        /// </summary>
        /// <param name="color">Color of the flow to be painted.</param>
        /// <param name="direction">Direction in which the flow will be painted.</param>
        public void AddFlow(Color color, Vector2Int direction)
        {
            if (circle.enabled || entranceFlow.enabled) setFlow(exitFlow, color, direction);
            else setFlow(entranceFlow, color, direction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onlyExit"></param>
        public void clearFlow(bool onlyExit)
        {
            clearWay(exitFlow);
            if (!onlyExit) clearWay(entranceFlow);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isCircle() { return circle.enabled; }

        public void setBackgroundColor(Color color)
        {
            // Sets the background color
            background.enabled = true;
            color.a = 0.25f;
            background.color = color;

        }
        public void removeBackgroundColor()
        {
            background.enabled = false;
        }

        //--------------------------------------------------------------------------//

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