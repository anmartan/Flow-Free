using System;
using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UILevelButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text _buttonText;
        [SerializeField] private Image _tick;

        private LevelData _levelData;
        
        public void OnClick()
        {
            GameManager.Instance().SetLevelData(_levelData);
            GameManager.Instance().ToLevelScene();
        }

        public void SetInformation(int category, int pack, int level)
        {
            _levelData.CategoryNumber = category;
            _levelData.PackNumber = pack;
            _levelData.LevelNumber = level;
            _levelData.BestSolve = 0;
            _levelData.State = LevelState.UNSOLVED;
            _levelData.Color = GameManager.Instance().GetCategories()[category].color;
            _levelData.Data = GameManager.Instance().GetCategories()[category].packs[pack].levels.ToString().Split('\n')[level];
            
            _buttonText.text = (_levelData.LevelNumber + 1).ToString();
        }

        internal void SetActive(bool active)
        {
            button.interactable = active;
        }

        // TODO
        public void ShowStar()
        {
            _tick.enabled = true;
            _tick.color = Color.yellow;
            
        }

        public void ShowTick()
        {
            _tick.enabled = true;
        }

        internal void SetColor(Color color)
        {
            button.image.color = color;
            _buttonText.color = Color.white;
        }
    }
}