using System.Collections;
using System.Collections.Generic;
using FlowFree;
using UnityEngine;
using UnityEngine.UI;

public class LevelCategory : MonoBehaviour
{
    [SerializeField] private Image _rectangleRenderer;
    [SerializeField] private Image _subrectangleRenderer;
    [SerializeField] private Text _titleText;

    [SerializeField] private LevelPackButton _buttonPrefab;
    
    public void InstantiateCategory(Category category)
    {
        _rectangleRenderer.color = category.shadeColor;
        _subrectangleRenderer.color = category.color;
        _titleText.text = category.categoryName;
        Debug.Log(category.categoryName);
        for (int i = 0; i < category.packs.Length; i++)
        {
            LevelPackButton button = Instantiate(_buttonPrefab, transform);
            button.SetInformation(category.packs[i]);
            button.SetColor(category.color);
        }
    }
}