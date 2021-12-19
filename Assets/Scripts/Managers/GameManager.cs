using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

namespace FlowFree
{
    // TODO: Comment everything
    // TODO: Remove unnecessary fields, methods, etc.
    public class GameManager : MonoBehaviour
    {
        string gameID = "4510487";
        
        private static GameManager _instance;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private LevelSelectorManager _levelSelectorManager;
        [SerializeField] private Category[] _levelCategories; // [TODO] llevar a un config.cs?
        [SerializeField] private Theme _theme;

        [SerializeField] private Interstitial_Ad _interstitialAd;
        [SerializeField] private Rewarded_Ad _rewardedAd;
        [SerializeField] private Banner_Ad _bannerAd;

        private int _hints = 3;
        private LevelData _currentLevelData;

        public static GameManager Instance()
        {
            return _instance;
        }
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

        public void ToLevelScene()
        {
            SceneManager.LoadScene("Level");
        }

        public void ToLevelSelectionScene()
        {
            SceneManager.LoadScene("Menu");
        }
        
        
        private void CreateLevel()
        {
            if(_levelManager) _levelManager.CreateLevel(_currentLevelData);
        }

        public bool IsThereAPreviousLevel()
        {
            return _currentLevelData.LevelNumber > 0;
        }

        public void PreviousLevel()
        {
            if(IsThereAPreviousLevel())
            {
                ChangeLevel(-1);
            }
        }

        public bool IsThereANextLevel()
        {
            var levels = _levelCategories[_currentLevelData.CategoryNumber].packs[_currentLevelData.PackNumber].levels.ToString().Split('\n');
            return _currentLevelData.LevelNumber + 1 < levels.Length - 1 && !NextLevelBlocked();
        }
        
        public void NextLevel()
        {
            
            if(IsThereANextLevel())
            {
                ChangeLevel(+1);
            }
        }

        private bool NextLevelBlocked()
        {
            bool blocked = _levelCategories[_currentLevelData.CategoryNumber].packs[_currentLevelData.PackNumber].blocked;
            if (!blocked) return false;
            
            DataManager.Instance().LoadLevel(GameManager.Instance().GetCategoryName(_currentLevelData.CategoryNumber), _currentLevelData.PackNumber, _currentLevelData.LevelNumber, out int steps, out bool perfect);
            return steps == -1;
        }

        private void ChangeLevel(int step)
        {
            var newLevelData = new LevelData();
            newLevelData.CategoryNumber = _currentLevelData.CategoryNumber;
            newLevelData.PackNumber = _currentLevelData.PackNumber;
            newLevelData.LevelNumber = _currentLevelData.LevelNumber+step;
            
            DataManager.Instance().LoadLevel(_levelCategories[newLevelData.CategoryNumber].name, newLevelData.PackNumber, newLevelData.LevelNumber, out int steps, out bool perfect);
            
            newLevelData.BestSolve = steps;
            newLevelData.State = LevelState.UNSOLVED;
            if (steps != -1) newLevelData.State = (perfect) ? LevelState.PERFECT : LevelState.SOLVED;
            newLevelData.Color = _levelCategories[newLevelData.CategoryNumber].color;
            newLevelData.Data = _levelCategories[newLevelData.CategoryNumber].packs[newLevelData.PackNumber].levels.ToString().Split('\n')[newLevelData.LevelNumber];

            _currentLevelData = newLevelData;
            _levelManager.CreateLevel(_currentLevelData);
        }


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
        public void UseHint()
        {
            _hints--;
            if (_hints < 0) _hints = 0;
        }
        public int GetHints() { return _hints; }

        public bool PlayIntersticialAd()
        {
            if (Advertisement.IsReady())
            {
                _interstitialAd.LoadAd();
                _interstitialAd.ShowAd();
                return true;
            }

            return false;
        }

        public bool PlayRewardedAd()
        {
            if (Advertisement.IsReady())
            {
                _rewardedAd.ShowAd();
                return true;
            }

            return false;
        }
        
        public bool PutBannerAdd()
        {
            if (Advertisement.IsReady())
            {
                _bannerAd.ShowBannerAd();
                return true;
            }

            return false;
        }

        public void FinishLevel(int steps, int minimumSteps)
        {
            DataManager.Instance().FinishLevel(_currentLevelData, steps, minimumSteps);
        }

        public void SetClues(int clues)
        {
            _hints = clues;
        }

        public void SetLevelData(LevelData data)
        {
            _currentLevelData = data;
        }
        public LevelData GetLevelData() { return _currentLevelData; }
    }
}
