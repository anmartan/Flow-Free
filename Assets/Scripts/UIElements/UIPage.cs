using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UIPage : MonoBehaviour
    {
        [Tooltip("Prefab of the level button, used to create each of the elements of the page.")]
        [SerializeField] private UILevelButton _buttonPrefab;                       // Prefab of the level button, used to create each of the elements of the page.
        
        [Tooltip("GridLayoutGroup used for creating the page.")]
        [SerializeField] private GridLayoutGroup _gridLayoutGroupConfiguration;     // GridLayoutGroup used for creating the page.

        /// <summary>
        /// Creates the page and sets its information.
        /// </summary>
        /// <param name="category">Category to which the levels belong.</param>
        /// <param name="pack">Pack to which the levels belong.</param>
        /// <param name="page">The number of the page (in the context of its pack).</param>
        /// <param name="color">The color used to paint the buttons in the page.</param>
        public void InstantiatePage(int category, int pack, int page, Color color)
        {
            // Checks if the first level is blocked: if the pack is not blocked, the level isn't either.
            bool blocked = GameManager.Instance().GetCategories()[category].packs[pack].blocked;
            int steps = (page == 0) ? 0 : -1; 
            
            // If the pack is blocked and it is not the first page, the number of steps is re-calculated to see if the first level should be unblocked.
            if(blocked && page > 0)
                DataManager.Instance().LoadLevel(GameManager.Instance().GetCategoryName(category), pack, page * 30, out steps, out bool perfect);
            
            // Each page has 30 elements.
            for (int i = 0; i < 30; i++)
            {
                // Creates a button, and assigns its level depending on the page.
                UILevelButton button = Instantiate(_buttonPrefab, transform);
                int levelNum = page * 30 + i;

                // Sets the level active if the pack is not blocked, or if the previous level was solved (steps != -1). 
                button.SetActive(!blocked || steps != -1); //A ver si me entero de lo de negar
                button.SetInformation(category, pack, levelNum);
                button.SetColor(color);
            }
        }

        /// <summary>
        /// Returns the width of the page, taking into account the spacing between elements and the size of each element.
        /// </summary>
        /// <returns>The width of the page.</returns>
        public float GetWidth()
        {
            int rowElements = _gridLayoutGroupConfiguration.constraintCount;
            float ret = _gridLayoutGroupConfiguration.cellSize.x * rowElements;
            ret += _gridLayoutGroupConfiguration.spacing.x * (rowElements - 1);

            return ret;
        }
    }
}