using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelSelectorManager : MonoBehaviour
    {
        /// <summary>
        /// Enum used to indicate which part of the menu it must show.
        /// </summary>
        private enum ShowState
        {
            MENU,
            CATEGORIES,
            PAGES
        };
        
        [Tooltip("Prefab for the categories that will be instantiated.")]
        [SerializeField] private UICategory _categoryPrefab;                            // Prefab for the categories that will be instantiated.
        
        [Tooltip("Prefab for the pages that will be instantiated.")]
        [SerializeField] private UIPage _pagePrefab;                                    // Prefab for the pages that will be instantiated.
        
        [Tooltip("Transform where the categories will be saved in the hierarchy.")]
        [SerializeField] private RectTransform _UICategoriesParent;                     // Transform where the categories will be saved in the hierarchy.
        
        [Tooltip("Transform where the pages will be saved in the hierarchy.")]
        [SerializeField] private RectTransform _UIPagesParent;                          // Transform where the pages will be saved in the hierarchy.

        [Tooltip("Vertical Layout Group used for organizing the categories.")]
        [SerializeField] private VerticalLayoutGroup _verticalLayoutConfiguration;      // Vertical Layout Group used for organizing the categories.

        [Tooltip("Horizontal Layout Group used for organizing the pages.")]
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutConfiguration;  // Horizontal Layout Group used for organizing the pages.

        [Tooltip("GameObject with the main menu elements.")]
        [SerializeField] private GameObject _mainMenu;                                  // GameObject with the main menu elements.
        
        [Tooltip("GameObject with the categories header.")]
        [SerializeField] private GameObject _header;                                    // GameObject with the categories header.
        
        [Tooltip("Text used for visualizing the category name (when showing the pages).")]
        [SerializeField] private Text _categoryNameText;                                // Text used for visualizing the category name (when showing the pages).

        [Tooltip("Button to go back in the scene.")] 
        [SerializeField] private GameObject _backButton;                                // Button to go back in the scene.
        
        private float _initialLayoutWidth;                                              // The initial width of the horizontalLayoutGroup.
        private ShowState _showState;                                                   // State that indicates what the game should show.
        
        /// <summary>
        /// Creates the categories and hides them if necessary.
        /// </summary>
        private void Start()
        {
            // Creates the categories and hides them until the player hits the play button.
            Category[] categories = GameManager.Instance().GetCategories();

            int offsetY = _verticalLayoutConfiguration.padding.vertical;
            for (int i = 0; i < categories.Length; i++)
            {
                UICategory newCategory = Instantiate(_categoryPrefab, _UICategoriesParent);
                newCategory.InstantiateCategory(categories[i], i);
                offsetY += (categories[i].packs.Length) * 160;
            }
            _UICategoriesParent.offsetMin = new Vector2(0, _UICategoriesParent.rect.height - offsetY);
            
            // Gets the initial layout width.
            _initialLayoutWidth = _UIPagesParent.rect.width;
            
            // Shows the information depending on its state.
            if(_showState == ShowState.MENU) ShowMainMenu();
            else if (_showState == ShowState.CATEGORIES) ShowCategories();
            else
            {
                LevelData data = GameManager.Instance().GetLevelData();
                ShowPages(data.CategoryNumber, data.PackNumber, data.Color);
            }
        }

        /// <summary>
        /// Shows the main menu, and makes everything else invisible.
        /// </summary>
        private void ShowMainMenu()
        {
            _mainMenu.SetActive(true);
            
            _header.SetActive(false);
            _UICategoriesParent.gameObject.SetActive(false);
            _UIPagesParent.gameObject.SetActive(false);
            _categoryNameText.gameObject.SetActive(false);
            _backButton.SetActive(false);

            _showState = ShowState.MENU;
        }
        
        /// <summary>
        /// Shows the categories menu, and makes everything else invisible.
        /// </summary>
        public void ShowCategories()
        {
            _mainMenu.SetActive(false);
            
            _header.SetActive(true);
            _UICategoriesParent.gameObject.SetActive(true);
            _UIPagesParent.gameObject.SetActive(false);
            _categoryNameText.gameObject.SetActive(false);
            _backButton.SetActive(true);
            
            _showState = ShowState.CATEGORIES;
        }

        /// <summary>
        /// Shows the pages menu, and makes everything else invisible.
        /// </summary>
        /// <param name="category">Category of the levels that will be seen.</param>
        /// <param name="pack">Pack of the levels that will be seen.</param>
        /// <param name="color">Color of the category, used to paint the buttons.</param>
        public void ShowPages(int category, int pack, Color color)
        {
            // Destroys the previous elements.
            for (int i = 0; i < _UIPagesParent.childCount; i++) Destroy(_UIPagesParent.GetChild(i).gameObject);

            Category[] categories = GameManager.Instance().GetCategories();
         
            // Restores its size.
            _UIPagesParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialLayoutWidth);
            
            // Checks that the info given is not incorrect (that should never happen).
            if (category >= categories.Length) return;
            if (pack >= categories[category].packs.Length) return;
            
            // Creates the pages for this pack.
            float offsetX = _horizontalLayoutConfiguration.padding.horizontal;
            int pagesNum = (categories[category].packs[pack].levels.ToString().Split('\n').Length) / 30;
            for (int i = 0; i < pagesNum; i++)
            {
                UIPage newPage = Instantiate(_pagePrefab, _UIPagesParent);
                newPage.InstantiatePage(category, pack, i, color);

                offsetX += newPage.GetWidth() + _horizontalLayoutConfiguration.spacing;
            }
            
            // Sets the size of the scroll, and sets its position to the origin, so that it is seen from the start.
            _UIPagesParent.offsetMin = new Vector2(_initialLayoutWidth -offsetX, 0);
            _UIPagesParent.SetPositionAndRotation(new Vector3(0,0 ,_UIPagesParent.transform.position.z), Quaternion.identity);

            // Switches the visibility of the objects.
            _mainMenu.SetActive(false);
            _header.SetActive(false);
            _UICategoriesParent.gameObject.SetActive(false);
            _UIPagesParent.gameObject.SetActive(true);
            _categoryNameText.gameObject.SetActive(true);
            _backButton.SetActive(true);
            
            _categoryNameText.text = categories[category].packs[pack].packName;
            _categoryNameText.color = color;

            _showState = ShowState.PAGES;
        }
        
        /// <summary>
        /// Callback for the "go back" button. It makes the screens appear in inverse order.
        /// </summary>
        public void GoBack()
        {
            if(_showState == ShowState.PAGES) ShowCategories();
            else ShowMainMenu();
        }

        /// <summary>
        /// Marks the initial state as the ShowPages one.
        /// </summary>
        public void StartShowingPages()
        {
            _showState = ShowState.PAGES;
        }

    }
}
