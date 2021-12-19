using UnityEngine;
using UnityEngine.UI;

namespace FlowFree
{
    public class UILevelButton : MonoBehaviour
    {
        [Tooltip("Button to which this component is attached.")]
        [SerializeField] private Button _button;                 // Button to which this component is attached.
        
        [Tooltip("Button text, where the number of the level will appear.")]
        [SerializeField] private Text _buttonText;              // Button text, where the number of the level will appear.
        
        [Tooltip("The sprite that will be used when the player finishes a level.")]
        [SerializeField] private Image _solvedImage;            // The sprite that will be used when the player finishes a level.
        
        [Tooltip("The sprite that will be used when the player finishes a level with a perfect score.")]
        [SerializeField] private Image _perfectSolvedImage;     // The sprite that will be used when the player finishes a level with a perfect score.
        
        [Tooltip("The sprite that will be used to indicate that a level is blocked.")]
        [SerializeField] private Image _blockedImage;           // The sprite that will be used to indicate that a level is blocked.

        private LevelData _levelData;                           // Level information attached to this button.
        
        /// <summary>
        /// What happens when a button is clicked: it sets the level to be played, and starts the game.
        /// </summary>
        public void OnClick()
        {
            GameManager.Instance().SetLevelData(_levelData);
            GameManager.Instance().ToLevelScene();
        }

        /// <summary>
        /// Updates the information of the button, so that it stores a correct levelData, which can be used to start a level.
        /// </summary>
        /// <param name="category"> Category to which the level belongs.</param>
        /// <param name="pack">Pack to which the level belongs.</param>
        /// <param name="level">Level (in the context of a pack).</param>
        public void SetInformation(int category, int pack, int level)
        {
            // Sets the levelData information.
            _levelData.CategoryNumber = category;
            _levelData.PackNumber = pack;
            _levelData.LevelNumber = level;
            _levelData.Color = GameManager.Instance().GetCategories()[category].color;
            _levelData.Data = GameManager.Instance().GetCategories()[category].packs[pack].levels.ToString().Split('\n')[level];

            // Checks whether there has been a previous solve, and saves it as part of the information.
            DataManager.Instance().LoadLevel(GameManager.Instance().GetCategoryName(category), pack, level, out int steps, out bool perfect);
            _levelData.BestSolve = steps;
            _levelData.State = LevelState.UNSOLVED;
            if (steps != -1) _levelData.State = (perfect) ? LevelState.PERFECT : LevelState.SOLVED;
            
            // Changes the button appearance.
            _buttonText.text = (_levelData.LevelNumber + 1).ToString();
            if (_levelData.State == LevelState.SOLVED) _solvedImage.enabled = true;
            else if (_levelData.State == LevelState.PERFECT) _perfectSolvedImage.enabled = true;
        }

        /// <summary>
        /// Sets the level as playable (or not).
        /// </summary>
        /// <param name="active">Whether the level can be played or not.</param>
        internal void SetActive(bool active)
        {
            _button.interactable = active;
            _blockedImage.enabled = !active;
        }
        
        /// <summary>
        /// Sets a color for the button.
        /// </summary>
        /// <param name="color">Color for the button.</param>
        internal void SetColor(Color color)
        {
            _button.image.color = color;
            _buttonText.color = Color.white;
        }
    }
}