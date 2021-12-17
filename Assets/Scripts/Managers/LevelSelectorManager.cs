using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class LevelSelectorManager : MonoBehaviour
    {
        [SerializeField] private LevelCategory _categoryPrefab;
        [SerializeField] private LevelPage _pagePrefab;
        [SerializeField] private RectTransform _UICategoriesParent;
        [SerializeField] private RectTransform _UIPagesParent;

        private void Start()
        {
            Category[] categories = GameManager.Instance().GetAvailableCategories();

            int offsetY = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                LevelCategory newCategory = Instantiate(_categoryPrefab, _UICategoriesParent);
                newCategory.InstantiateCategory(categories[i], i);
                offsetY += (categories[i].packs.Length) * 160;
            }
            _UICategoriesParent.offsetMin = new Vector2(0, _UICategoriesParent.rect.height - offsetY);
        }

        public void ShowCategories()
        {
            _UICategoriesParent.gameObject.SetActive(true);
            _UIPagesParent.gameObject.SetActive(false);
        }

        public void ShowPages(int category, int pack)
        {
            for (int i = 0; i < _UIPagesParent.childCount; i++) Destroy(_UIPagesParent.GetChild(i).gameObject);
            
            Category[] categories = GameManager.Instance().GetAvailableCategories();
            
            // Checks that the info given is not incorrect (that should never happen).
            if (category >= categories.Length) return;
            if (pack >= categories[category].packs.Length) return;
            
            // Creates the pages for this pack.
            int offsetX = 0;
            int pagesNum = categories[category].packs[pack].levels.ToString().Split('\n').Length;
            for (int i = 0; i < pagesNum /30; i++)
            {
                LevelPage newPage = Instantiate(_pagePrefab, _UIPagesParent);
                newPage.InstantiatePage(category, pack, i);
            }
            _UICategoriesParent.offsetMin = new Vector2(0, _UICategoriesParent.rect.height - offsetX);
            
            _UICategoriesParent.gameObject.SetActive(false);
            _UIPagesParent.gameObject.SetActive(true);
        }
    }
}
