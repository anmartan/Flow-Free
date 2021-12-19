using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UIPackButton : MonoBehaviour
    {
        [SerializeField] private Text _packName;
        [SerializeField] private Text _levels;

        private int _categoryIndex;
        private int _packIndex;

        private Color _color;

        public void SetInformation(LevelPack pack, int categoryIndex, int packIndex)
        {
            _packName.text = pack.packName;
            int levelsNum = (pack.levels.ToString().Split('\n')).Length - 1;
            _levels.text = DataManager.Instance().GetPackCompletedLevels(GameManager.Instance().GetCategoryName(categoryIndex), packIndex) + "/" + levelsNum;
            _categoryIndex = categoryIndex;
            _packIndex = packIndex;
        }

        public void SetColor(Color color)
        {
            _packName.color = color;
            _color = color;
        }

        public void OnClick()
        {
            GameManager.Instance().GetLevelSelectorManager().ShowPages(_categoryIndex, _packIndex, _color);
        }
    }
}
