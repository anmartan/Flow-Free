using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UIPackButton : MonoBehaviour
    {
        [Tooltip("Text where the name of the pack will appear.")]
        [SerializeField] private Text _packName;    // Text where the name of the pack will appear.
        
        [Tooltip("Text where the number of completed levels will appear.")]
        [SerializeField] private Text _levels;      // Text where the number of completed levels will appear.

        private int _categoryIndex;                 // Category to which this pack belongs.
        private int _packIndex;                     // Order in which the pack appears (in the context of its category).
        private Color _color;                       // Color that represents this category.

        /// <summary>
        /// Sets the information for this pack.
        /// </summary>
        /// <param name="pack">Scriptable object from which the information will be read.</param>
        /// <param name="categoryIndex">Index of the category to which it belongs.</param>
        /// <param name="packIndex">Order in which a the pack appears (in the context of its category).</param>
        public void SetInformation(LevelPack pack, int categoryIndex, int packIndex)
        {
            _categoryIndex = categoryIndex;
            _packName.text = pack.packName;
            _packIndex = packIndex;
            
            // Sets the text to "completed / total".
            int levelsNum = (pack.levels.ToString().Split('\n')).Length - 1;
            _levels.text = DataManager.Instance().GetPackCompletedLevels(GameManager.Instance().GetCategoryName(categoryIndex), packIndex) + "/" + levelsNum;
        }

        /// <summary>
        /// Sets the color of the category (used for the buttons and the grid, later on).
        /// </summary>
        /// <param name="color">Color for the pack.</param>
        public void SetColor(Color color)
        {
            _packName.color = color;
            _color = color;
        }

        /// <summary>
        /// What happens when the player clicks a pack: it shows the pages it contains.
        /// </summary>
        public void OnClick()
        {
            GameManager.Instance().GetLevelSelectorManager().ShowPages(_categoryIndex, _packIndex, _color);
        }
    }
}
