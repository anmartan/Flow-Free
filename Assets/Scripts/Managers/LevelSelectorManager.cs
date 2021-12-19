using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelSelectorManager : MonoBehaviour
    {
        public enum ShowState
        {
            MENU,
            CATEGORIES,
            PAGES
        };
        
        [SerializeField] private UICategory _categoryPrefab;
        [SerializeField] private UIPage _pagePrefab;
        [SerializeField] private RectTransform _UICategoriesParent;
        [SerializeField] private RectTransform _UIPagesParent;

        [SerializeField] private VerticalLayoutGroup _verticalLayoutConfiguration;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutConfiguration;

        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _header;
        [SerializeField] private Text _categoryNameText;
        
        private float _initialLayoutWidth;
        private ShowState _showState;
        private void Start()
        {
            // Creates the categories and hides them until the player hits the play button
            Category[] categories = GameManager.Instance().GetCategories();

            int offsetY = _verticalLayoutConfiguration.padding.vertical;
            for (int i = 0; i < categories.Length; i++)
            {
                UICategory newCategory = Instantiate(_categoryPrefab, _UICategoriesParent);
                newCategory.InstantiateCategory(categories[i], i);
                offsetY += (categories[i].packs.Length) * 160;
            }
            _UICategoriesParent.offsetMin = new Vector2(0, _UICategoriesParent.rect.height - offsetY);


            _initialLayoutWidth = _UIPagesParent.rect.width;
            
            if(_showState == ShowState.MENU) ShowMainMenu();
            else if (_showState == ShowState.CATEGORIES) ShowCategories();
            else
            {
                LevelData data = GameManager.Instance().GetLevelData();
                ShowPages(data.CategoryNumber, data.PackNumber, data.Color);
            }
        }

        private void ShowMainMenu()
        {
            _mainMenu.SetActive(true);
            
            _header.SetActive(false);
            _UICategoriesParent.gameObject.SetActive(false);
            _UIPagesParent.gameObject.SetActive(false);
            _categoryNameText.gameObject.SetActive(false);

            _showState = ShowState.MENU;
        }
        public void ShowCategories()
        {
            _mainMenu.SetActive(false);
            
            _header.SetActive(true);
            _UICategoriesParent.gameObject.SetActive(true);
            _UIPagesParent.gameObject.SetActive(false);
            _categoryNameText.gameObject.SetActive(false);
            
            _showState = ShowState.CATEGORIES;

            _UIPagesParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialLayoutWidth);
        }

        public void ShowPages(int category, int pack, Color color)
        {
            for (int i = 0; i < _UIPagesParent.childCount; i++) Destroy(_UIPagesParent.GetChild(i).gameObject);

            Category[] categories = GameManager.Instance().GetCategories();
            
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
            _categoryNameText.text = categories[category].packs[pack].packName;
            _categoryNameText.color = color;

            _showState = ShowState.PAGES;
        }


        public void GoBack()
        {
            if(_showState == ShowState.PAGES) ShowCategories();
            else ShowMainMenu();
        }

        public void ShowPages()
        {
            _showState = ShowState.PAGES;
        }

    }
}
