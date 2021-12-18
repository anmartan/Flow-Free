using System.Collections;
using System.Collections.Generic;
using FlowFree;
using UnityEngine;
using UnityEngine.UI;

public class UIPackButton : MonoBehaviour
{
    [SerializeField] private Text _packName;
    [SerializeField] private Text _levels;

    private int _categoryIndex;
    private int _packIndex;
    
    private LevelPack _levelPack;
    private int _levelsSolved;

    public void SetInformation(LevelPack pack, int categoryIndex, int packIndex)
    {
        _levelPack = pack;
        _packName.text = pack.packName;
        int levelsNum = (pack.levels.ToString().Split('\n')).Length - 1;
        _levels.text = _levelsSolved + "/" + levelsNum;
        _categoryIndex = categoryIndex;
        _packIndex = packIndex;
    }

    public void SetColor(Color color)
    {
        _packName.color = color;
    }

    public void OnClick()
    {
        GameManager.Instance().GetLevelSelectorManager().ShowPages(_categoryIndex, _packIndex);
    }
}