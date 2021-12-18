using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UILevelButton : MonoBehaviour
    {
        [SerializeField] private Text _buttonText;
        [SerializeField] private Image _tick;

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
            _buttonText.text = (_levelInPage + 1).ToString();
        }

        public void ShowStar()
        {
            
        }

        public void ShowTick()
        {
            _tick.enabled = true;
        }
    }
}