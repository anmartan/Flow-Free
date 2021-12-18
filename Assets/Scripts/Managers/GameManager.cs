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
        public int levelCat = 0;
        public int levelPack = 0;
        public int levelNum = 0;

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
                // TODO: Erase
                levelCat = _instance.levelCat;
                levelPack = _instance.levelPack ;
                levelNum = _instance.levelNum;
                
                // Assigns the level manager and board manager that will be used in the new scene.
                _instance._levelManager = _levelManager;
                _instance._boardManager = _boardManager;
                _instance._levelSelectorManager = _levelSelectorManager;

                Destroy(this);
            }

            if (_instance._boardManager) CreateLevel();
        }

        public void NextLevel()
        {
            levelNum++;
            CreateLevel();
        }
        public void ToLevelScene()
        {
            SceneManager.LoadScene("Level");
        }

        public void ToLevelSelectionScene()
        {
            SceneManager.LoadScene("LevelSelection");
        }
        
        public void PreviousLevel()
        {
            levelNum--;
            CreateLevel();
        }
        
        private void CreateLevel()
        {
            TextAsset text = _levelCategories[levelCat].packs[levelPack].levels;
            string[] levels = text.ToString().Split('\n');

            LevelData lel = new LevelData();
            lel.LevelNumber = levelNum;
            lel.PackNumber = levelPack;
            lel.CategoryNumber = levelCat;
            lel.Data = levels[levelNum];
            lel.Color = _levelCategories[levelCat].color;
            lel.BestSolve = 0;
            lel.State = LevelState.UNSOLVED;
            if(_levelManager) _levelManager.CreateLevel(lel);
        }

        // TODO luego se quita?
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                DataManager.Instance().Save();
            }
        }

        public Theme getActualTheme() { return _theme; }

        public LevelSelectorManager GetLevelSelectorManager() { return _levelSelectorManager; }
        public LevelManager GetLevelManager() { return _levelManager; }
        public BoardManager GetBoardManager() { return _boardManager; }
        public Category[] GetCategories()
        {
            return _levelCategories;
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
                _rewardedAd.LoadAd();
                _rewardedAd.ShowAd();
                return true;
            }

            return false;
        }
        
        public bool PutBannerAdd()
        {
            if (Advertisement.IsReady())
            {
                _bannerAd.LoadBanner();
                _bannerAd.ShowBannerAd();
                return true;
            }

            return false;
        }

        public void FinishLevel(int steps)
        {
            DataManager.Instance().FinishLevel(_levelCategories[levelCat].categoryName, levelPack, levelNum, steps);
        }

        public void SetClues(int clues)
        {
            _hints = clues;
        }
    }
}
