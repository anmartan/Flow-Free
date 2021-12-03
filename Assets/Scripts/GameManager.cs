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
        [SerializeField] private LevelCategory[] levelCategory; // [TODO] llevar a un config.cs?
        [SerializeField] private Theme[] themes;                // algo que se encargue de estas cosas

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

        }

        private void Start()
        {
            TextAsset text = levelCategory[0].packages[0].levels;
            string str = text.ToString();
            Debug.Log(str);
            Map level = new Map();
            if (level.loadMap(str))

                boardManager.createBoard(level);
            else Debug.LogError("Nivel incorrecto");
        }


        // [TODO] getter pero bien
        public Theme getActualTheme()
        {
            return themes[0];
        }
    }
}
