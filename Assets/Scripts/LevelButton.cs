using System.Collections;
using System.Collections.Generic;
using FlowFree;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Text _buttonText;
    
    private int _levelCategory;
    private int _levelPack;
    private int _levelInPage;
    
    public void OnClick()
    {
        GameManager.Instance().levelCat = _levelCategory;
        GameManager.Instance().levelPack = _levelPack;
        GameManager.Instance().levelNum = _levelInPage;
        GameManager.Instance().ToLevelScene();
    }

    public void SetInformation(int category, int pack, int level)
    {
        _levelCategory = category;
        _levelPack = pack;
        _levelInPage = level;
        _buttonText.text = (level + 1).ToString();
    }
}
