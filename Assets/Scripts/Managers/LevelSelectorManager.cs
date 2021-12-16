using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class LevelSelectorManager : MonoBehaviour
    {
        [SerializeField] private LevelCategory _categoryPrefab;
        [SerializeField] private RectTransform _rectTransform;
        
        private void Start()
        {
            Category[] categories = GameManager.Instance().GetAvailableCategories();

            int objectsCrated = 0;
            for (int i = 0; i < categories.Length; i++)
            {
                LevelCategory newCategory = Instantiate(_categoryPrefab, transform);
                newCategory.InstantiateCategory(categories[i]);
                objectsCrated += (categories[i].packs.Length + 1) * 160; 
            }
            
            _rectTransform.offsetMin = new Vector2(0, _rectTransform.rect.height - objectsCrated);
        }
    }
}
