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
            bool blocked = GameManager.Instance().GetCategories()[category].packs[pack].blocked;
            int steps = (page == 0) ? 0 : -1; 
            if(blocked && page > 0)
                DataManager.Instance().LoadLevel(GameManager.Instance().GetCategoryName(category), pack, page * 30, out steps, out bool _);


            for (int i = 0; i < 30; i++)
            {
                UILevelButton button = Instantiate(_buttonPrefab, transform);
                int levelNum = page * 30 + i;

                //Set active if no bloqued or steps different from -1 (first page or previous level completed) 
                button.SetActive(!blocked || steps != -1); //A ver si me entero de lo de negar
                DataManager.Instance().LoadLevel(GameManager.Instance().GetCategoryName(category), pack, levelNum, out steps, out bool perfect);
                if (perfect) button.ShowStar(); 
                else if(steps != -1) button.ShowTick();
                button.SetInformation(category, pack, levelNum);
                button.SetColor(color);
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