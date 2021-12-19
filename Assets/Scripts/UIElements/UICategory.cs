using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UICategory : MonoBehaviour
    {
        [Tooltip("Background for the UI element.")]
        [SerializeField] private Image _rectangleRenderer;          // Background for the UI element.
        
        [Tooltip("Line in the bottom part of the object, in a lighter color.")]
        [SerializeField] private Image _subrectangleRenderer;       // Line in the bottom part of the object, in a lighter color.
        
        [Tooltip("Text where the name of the category will appear.")]
        [SerializeField] private Text _titleText;                   // Text where the name of the category will appear.

        [Tooltip("Button prefab that will be used for each of the packs in a category.")]
        [SerializeField] private UIPackButton _buttonPrefab;        // Button prefab that will be used for each of the packs in a category.
        
        [Tooltip("Background of the button that will be used for the packs in a category.")]
        [SerializeField] private SpriteRenderer _buttonBackground;  // Background of the button that will be used for the packs in a category.
        
        /// <summary>
        /// Creates the categories for the UI, with all of the packs it has.
        /// </summary>
        /// <param name="category">The scriptable object from which it will read the information.</param>
        /// <param name="categoryIndex">The order in which this category appears in the array of categories.</param>
        public void InstantiateCategory(Category category, int categoryIndex)
        {
            // Sets its colors and name, reading from the scriptable object.
            _rectangleRenderer.color = category.shadeColor;
            _subrectangleRenderer.color = category.color;
            _titleText.text = category.categoryName;

            // Creates as many pack buttons as necessary, and sets their information and color.
            for (int i = 0; i < category.packs.Length; i++)
            {
                UIPackButton button = Instantiate(_buttonPrefab, transform);
                button.SetInformation(category.packs[i], categoryIndex, i);
                button.SetColor(category.color);
            }
        }
    }
}
