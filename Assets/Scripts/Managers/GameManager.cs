using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlowFree
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private Category[] _levelCategories; // [TODO] llevar a un config.cs?
        [SerializeField] private Theme[] themes;                // algo que se encargue de estas cosas

        private int _hints = 3;
        public int levelCat = 0;
        public int levelPack = 0;
        public int levelNum = 0;

        public static GameManager Instance()
        {
            return instance;
        }
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                // Assigns the level manager and board manager that will be used in the new scene.
                instance.levelManager = levelManager;
                instance.boardManager = boardManager;

                Destroy(this);
            }

            if (instance.boardManager) createLevel();
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
            if(levelManager) levelManager.CreateLevel(lel);
        }
        
        // [TODO] getter pero bien
        public Theme getActualTheme()
        {
            return themes[0];
        }

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
    }
}
