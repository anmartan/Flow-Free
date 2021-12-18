using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UIPage : MonoBehaviour
    {
        [SerializeField] private UILevelButton _buttonPrefab;
        [SerializeField] private GridLayoutGroup _gridLayoutGroupConfiguration;

        public void InstantiatePage(int category, int pack, int page, Color color)
        {
            for (int i = 0; i < 30; i++)
            {
                UILevelButton button = Instantiate(_buttonPrefab, transform);
                int levelNum = page * 30 + i;
                int steps;
                DataManager.Instance().LoadLevel(GameManager.Instance().GetCategories()[category].categoryName, pack, levelNum, out steps);
                if(steps != -1) button.ShowTick();
                button.SetInformation(category, pack, levelNum);
                // TODO: Set _color
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
}