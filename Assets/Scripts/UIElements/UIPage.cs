using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPage : MonoBehaviour
{
    [SerializeField] private UILevelButton _buttonPrefab;
    [SerializeField] private GridLayoutGroup _gridLayoutGroupConfiguration;

    public void InstantiatePage(int category, int pack, int page, Color color)
    {
        for (int i = 0; i < 30; i++)
        {
            UILevelButton button = Instantiate(_buttonPrefab, transform);
            button.SetInformation(category, pack, page * 30 + i);
            // TODO: Set color
        }
    }

    public float GetWidth()
    {
        int rowElements = _gridLayoutGroupConfiguration.constraintCount;
        float ret = _gridLayoutGroupConfiguration.cellSize.x * rowElements;
        ret += _gridLayoutGroupConfiguration.spacing.x * (rowElements - 1);

        return ret;
    }
}
