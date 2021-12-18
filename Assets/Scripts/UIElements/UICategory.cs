using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UICategory : MonoBehaviour
    {
        [SerializeField] private Image _rectangleRenderer;
        [SerializeField] private Image _subrectangleRenderer;
        [SerializeField] private Text _titleText;

        [SerializeField] private UIPackButton _buttonPrefab;
        [SerializeField] private SpriteRenderer _buttonBackground;

        private Color _categoryColor;

        public void InstantiateCategory(Category category, int categoryIndex)
        {
            _categoryColor = category.color;
            _rectangleRenderer.color = category.shadeColor;
            _subrectangleRenderer.color = category.color;
            _titleText.text = category.categoryName;

            for (int i = 0; i < category.packs.Length; i++)
            {
                UIPackButton button = Instantiate(_buttonPrefab, transform);
                button.SetInformation(category.packs[i], categoryIndex, i);
                button.SetColor(category.color);
            }
        }

        public float GetHeight()
        {
            float ret = transform.childCount * _buttonBackground.size.y;
            ret += _rectangleRenderer.sprite.rect.height;

            return ret;
        }
    }
}
