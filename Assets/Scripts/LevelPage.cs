using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPage : MonoBehaviour
{
    [SerializeField] private LevelButton _buttonPrefab;


    public void InstantiatePage(int category, int pack, int page)
    {
        for (int i = 0; i < 30; i++)
        {
            LevelButton button = Instantiate(_buttonPrefab, transform);
            button.SetInformation(category, pack, page * 30 + i);
        }
    }
}
