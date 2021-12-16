using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelPackButton : MonoBehaviour
{
    [SerializeField] private Text _packName;
    [SerializeField] private Text _levels;

    private LevelPack _levelPack;
    private int _levelsSolved;

    public void SetInformation(LevelPack pack)
    {
        _levelPack = pack;
        _packName.text = pack.packName;
        int levelsNum = (pack.levels.ToString().Split('\n')).Length - 1;
        _levels.text = _levelsSolved + "/" + levelsNum;
    }

    public void SetColor(Color color)
    {
        _packName.color = color;
    }
}
