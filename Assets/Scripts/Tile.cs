using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FlowFree
{
    public class Tile : MonoBehaviour
    {
        [Tooltip("Sprite used for drawing the flow in each tile.")]
        [SerializeField] private SpriteRenderer flow;

        [Tooltip("Sprite used for drawing the circles in the needed tiles.")]
        [SerializeField] private SpriteRenderer circle;

        private bool isCircle = false;  // whether the tile is one of the ends of a flow or not


        /// <summary>
        /// Makes the tile have a circle, with the specified color.
        /// </summary>
        /// <param name="color">Color of the circle. This cannot be changed afterwards.</param>
        public void PutCircle(Color color)
        {
            // Check so that a circle is not changed after it's been set
            if(!isCircle)
            {
                isCircle = true;
                circle.enabled = true;
                circle.color = color;

            }
        }

        //private void OnMouseDrag()
        //{
        //    if (isCircle)
        //    {
        //        circle.color += new Color(0.01f, 0.01f, 0.01f);
        //    }
        //}
    }
}