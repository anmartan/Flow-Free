using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class LevelSelectorManager : MonoBehaviour
    {
        [SerializeField] private UICategory _categoryPrefab;
        [SerializeField] private UIPage _pagePrefab;
        [SerializeField] private RectTransform _UICategoriesParent;
        [SerializeField] private RectTransform _UIPagesParent;

        [SerializeField] private VerticalLayoutGroup _verticalLayoutConfiguration;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutConfiguration;

        private float _initialLayoutWidth;
        private bool _showingLevels;
        private void Start()
        {
            _showingLevels = false;
            Category[] categories = GameManager.Instance().GetAvailableCategories();

            int offsetY = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                UICategory newCategory = Instantiate(_categoryPrefab, _UICategoriesParent);
                newCategory.InstantiateCategory(categories[i], i);
                offsetY += (categories[i].packs.Length) * 160;
            }
            _UICategoriesParent.offsetMin = new Vector2(0, _UICategoriesParent.rect.height - offsetY);


            _initialLayoutWidth = _UIPagesParent.rect.width;
        }

        public void ShowCategories()
        {
            _UICategoriesParent.gameObject.SetActive(true);
            _UIPagesParent.gameObject.SetActive(false);
            _showingLevels = false;

            _UIPagesParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _initialLayoutWidth);
        }

        public void ShowPages(int category, int pack)
        {
            for (int i = 0; i < _UIPagesParent.childCount; i++) Destroy(_UIPagesParent.GetChild(i).gameObject);

            Category[] categories = GameManager.Instance().GetAvailableCategories();
            
            // Checks that the info given is not incorrect (that should never happen).
            if (category >= categories.Length) return;
            if (pack >= categories[category].packs.Length) return;
            
            // Creates the pages for this pack.
            float offsetX = _horizontalLayoutConfiguration.padding.horizontal;
            int pagesNum = (categories[category].packs[pack].levels.ToString().Split('\n').Length) / 30;
            for (int i = 0; i < pagesNum; i++)
            {
                UIPage newPage = Instantiate(_pagePrefab, _UIPagesParent);
                newPage.InstantiatePage(category, pack, i);

                offsetX += newPage.GetWidth() + _horizontalLayoutConfiguration.spacing;
            }
            
            // Sets the size of the scroll, and sets its position to the origin, so that it is seen from the start.
            _UIPagesParent.offsetMin = new Vector2(_initialLayoutWidth -offsetX, 0);
            _UIPagesParent.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            // Switches the visibility of the two objects.
            _UICategoriesParent.gameObject.SetActive(false);
            _UIPagesParent.gameObject.SetActive(true);

            _showingLevels = true;
        }


        public void GoBack()
        {
            if(_showingLevels) ShowCategories();
            else GameManager.Instance().ToMenu();
        }
    }
}
