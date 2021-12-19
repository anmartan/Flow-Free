using System;
using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UILevelButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text _buttonText;
        [SerializeField] private Image _solvedImage;
        [SerializeField] private Image _perfectSolvedImage;
        [SerializeField] private Image _blockedImage;

        private LevelData _levelData;
        
        public void OnClick()
        {
            GameManager.Instance().SetLevelData(_levelData);
            GameManager.Instance().ToLevelScene();
        }

        public void SetInformation(int category, int pack, int level, int steps, bool perfect)
        {
            _levelData.CategoryNumber = category;
            _levelData.PackNumber = pack;
            _levelData.LevelNumber = level;
            _levelData.BestSolve = steps;
            _levelData.State = LevelState.UNSOLVED;
            if (steps != -1) _levelData.State = (perfect) ? LevelState.PERFECT : LevelState.SOLVED;
            _levelData.Color = GameManager.Instance().GetCategories()[category].color;
            _levelData.Data = GameManager.Instance().GetCategories()[category].packs[pack].levels.ToString().Split('\n')[level];
            
            _buttonText.text = (_levelData.LevelNumber + 1).ToString();
        }

        internal void SetActive(bool active)
        {
            button.interactable = active;
            _blockedImage.enabled = !active;
        }

        public void ShowStar()
        {
            _perfectSolvedImage.enabled = true;
        }

        public void ShowTick()
        {
            _solvedImage.enabled = true;
        }

        internal void SetColor(Color color)
        {
            button.image.color = color;
            _buttonText.color = Color.white;
        }
    }
}