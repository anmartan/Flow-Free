using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;

namespace FlowFree
{
    public class GameManager : MonoBehaviour
    {
        string gameID = "4510487";
        
        private static GameManager _instance;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private LevelSelectorManager _levelSelectorManager;
        [SerializeField] private Category[] _levelCategories; // [TODO] llevar a un config.cs?
        [SerializeField] private Theme[] _themes;                // algo que se encargue de estas cosas

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

            if (_instance._boardManager) createLevel();
        }

        public void NextLevel()
        {
            levelNum++;
            createLevel();
        }

        public void ToLevelScene()
        {
            SceneManager.LoadScene("Level");
        }

        public void PreviousLevel()
        {
            levelNum--;
            createLevel();
        }
        private void createLevel()
        {
            TextAsset text = _levelCategories[levelCat].packs[levelPack].levels;
            string[] levels = text.ToString().Split('\n');

            LevelData lel;
            lel.color = _levelCategories[levelCat].color;
            lel.levelNumber = levelNum;
            lel.level = levels[levelNum];
            if(_levelManager) _levelManager.CreateLevel(lel);
        }
        
        // [TODO] getter pero bien
        public Theme getActualTheme()
        {
            return _themes[0];
        }

        public LevelSelectorManager GetLevelSelectorManager() { return _levelSelectorManager; }
        public Category[] GetAvailableCategories()
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
    }
}
