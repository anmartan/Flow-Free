using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

namespace FlowFree
{
    public class GameManager : MonoBehaviour
    {
        string gameID = "4510487";                                              // Game ID (for ads).
        
        [Tooltip("Instance of the level manager in the scene.")]
        [SerializeField] private LevelManager _levelManager;                    // Instance of the level manager in the scene.
        
        [Tooltip("Instance of the board manager in the scene.")]
        [SerializeField] private BoardManager _boardManager;                    // Instance of the board manager in the scene.
        
        [Tooltip("Instance of the level selector manager in the scene.")]
        [SerializeField] private LevelSelectorManager _levelSelectorManager;    // Instance of the level selector manager in the scene.
        
        [Tooltip("Categories that will be included in the game.")]
        [SerializeField] private Category[] _levelCategories;                   // Categories that will be included in the game.
        
        [Tooltip("Theme that will be used in the game.")]
        [SerializeField] private Theme _theme;                                  // Theme that will be used in the game.

        [SerializeField] private Interstitial_Ad _interstitialAd;
        [SerializeField] private Rewarded_Ad _rewardedAd;
        [SerializeField] private Banner_Ad _bannerAd;

        private int _hints = 3;                                                 // The number of hints the player has.
        private LevelData _currentLevelData;                                    // LevelData of the current level.

        private static GameManager _instance;
        
        /// <summary>
        /// Returns the instance of the GameManager.
        /// </summary>
        /// <returns>Instance of the GameManager.</returns>
        public static GameManager Instance()
        {
            return _instance;
        }
        
        /// <summary>
        /// Updates its attributes, and sets the values needed.
        /// </summary>
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                Advertisement.Initialize(gameID);
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                // Saves the level data that was given to the game manager before.
                _currentLevelData = _instance._currentLevelData;
                
                // Assigns the level manager and board manager that will be used in the new scene.
                _instance._levelManager = _levelManager;
                _instance._boardManager = _boardManager;
                _instance._levelSelectorManager = _levelSelectorManager;
                
                if(_levelSelectorManager) _levelSelectorManager.StartShowingPages();
                Destroy(this);
            }

            if (_instance._boardManager) CreateLevel();
        }

        /// <summary>
        /// Goes to the level scene.
        /// </summary>
        public void ToLevelScene()
        {
            SceneManager.LoadScene("Level");
        }

        /// <summary>
        /// Goes to the menu scene.
        /// </summary>
        public void ToMenuScene()
        {
            SceneManager.LoadScene("Menu");
        }
        
        /// <summary>
        /// Goes to the previous level, if there is one.
        /// </summary>
        public void PreviousLevel()
        {
            if(IsThereAPreviousLevel()) ChangeLevel(-1);
        }
        
        /// <summary>
        /// Goes to the next level, if there is one.
        /// </summary>
        public void NextLevel()
        {
            if(IsThereANextLevel()) ChangeLevel(+1);
        }
        
        /// <summary>
        /// Uses a hint.
        /// </summary>
        public void UseHint()
        {
            _hints--;
            if (_hints < 0) _hints = 0;
        }
        
        /// <summary>
        /// Adds a hint.
        /// </summary>
        public void AddHint()
        {
            _hints++;
            if(_levelManager) _levelManager.UpdateHintsButton();
        }
        
        /// <summary>
        /// Saves the level.
        /// </summary>
        /// <param name="steps">Steps the player needed to finish the level.</param>
        /// <param name="minimumSteps">Minimum amount of steps needed to solve the level.</param>
        public void FinishLevel(int steps, int minimumSteps)
        {
            DataManager.Instance().FinishLevel(_currentLevelData, steps, minimumSteps);
        }
        
        /// <summary>
        /// Shows an interstitial ad.
        /// </summary>
        public void PlayIntersticialAd()
        {
            _interstitialAd.ShowAd();
        }

        /// <summary>
        /// Shows a rewarded ad.
        /// </summary>
        public void PlayRewardedAd()
        {
            _rewardedAd.ShowAd();
        }

        // ----- GETTERS ----- //

        public Theme GetActualTheme() { return _theme; }
        public LevelSelectorManager GetLevelSelectorManager() { return _levelSelectorManager; }
        public LevelManager GetLevelManager() { return _levelManager; }
        public BoardManager GetBoardManager() { return _boardManager; }
        public Category[] GetCategories() { return _levelCategories; }
        public string GetCategoryName(int category)
        {
            if (category < 0 || category >= _levelCategories.Length) return "";
            return _levelCategories[category].categoryName;
        }
        public int GetHints() { return _hints; }
        public LevelData GetLevelData() { return _currentLevelData; }
        public bool IsThereAPreviousLevel() { return _currentLevelData.LevelNumber > 0; }
        public bool IsThereANextLevel()
        {
            var levels = _levelCategories[_currentLevelData.CategoryNumber].packs[_currentLevelData.PackNumber].levels.ToString().Split('\n');
            return _currentLevelData.LevelNumber + 1 < levels.Length - 1 && !NextLevelBlocked();
        }
        
        // ----- SETTERS ----- //
        
        public void SetClues(int clues) { _hints = clues; }
        public void SetLevelData(LevelData data) { _currentLevelData = data; }
        
        
        // ----- PRIVATE METHODS ----- //

        /// <summary>
        /// Creates the level with the current levelData.
        /// </summary>
        private void CreateLevel()
        {
            if(_levelManager) _levelManager.CreateLevel(_currentLevelData);
        }
        
        /// <summary>
        /// Checks if the next level is blocked.
        /// </summary>
        /// <returns>True if the next level is blocked; false otherwise.</returns>
        private bool NextLevelBlocked()
        {
            // If the pack is not blocked, the level cannot be.
            bool blocked = _levelCategories[_currentLevelData.CategoryNumber].packs[_currentLevelData.PackNumber].blocked;
            if (!blocked) return false;
            
            // Otherwise, checks if the level before that one was solved.
            DataManager.Instance().LoadLevel(_levelCategories[_currentLevelData.CategoryNumber].categoryName, _currentLevelData.PackNumber, _currentLevelData.LevelNumber, out int steps, out bool perfect);
            return steps == -1;
        }

        /// <summary>
        /// Changes to the level that is step levels ahead of the current one.
        /// </summary>
        /// <param name="step">The number of levels it will "skip".</param>
        private void ChangeLevel(int step)
        {
            var newLevelData = new LevelData
            {
                CategoryNumber = _currentLevelData.CategoryNumber,
                PackNumber = _currentLevelData.PackNumber,
                LevelNumber = _currentLevelData.LevelNumber+step,
                Color = _levelCategories[_currentLevelData.CategoryNumber].color,
                State = LevelState.UNSOLVED
            };

            DataManager.Instance().LoadLevel(_levelCategories[newLevelData.CategoryNumber].name, newLevelData.PackNumber, newLevelData.LevelNumber, out int steps, out bool perfect);
            
            // Sets the values that have to be loaded.
            newLevelData.BestSolve = steps;
            if (steps != -1) newLevelData.State = (perfect) ? LevelState.PERFECT : LevelState.SOLVED;
            newLevelData.Data = _levelCategories[newLevelData.CategoryNumber].packs[newLevelData.PackNumber].levels.ToString().Split('\n')[newLevelData.LevelNumber];

            // Updates the current LevelData and creates the new level.
            _currentLevelData = newLevelData;
            _levelManager.CreateLevel(_currentLevelData);
        }
    }
}
