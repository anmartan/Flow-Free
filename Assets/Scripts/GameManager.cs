using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowFree
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private BoardManager boardManager;
        
        
        // [TODO] Remove. Used for easy checks.
        public int width = 5;
        public int height = 5;

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

        }

        // [TODO]
        // Remove this. It is temporally used, until the level loading is implemented.
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                boardManager.createBoard(width, height);
            }
        }

    }
}
