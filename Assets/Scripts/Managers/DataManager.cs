using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Windows;

namespace FlowFree
{
    public class DataManager : MonoBehaviour
    {
        private static string _path;                // Path where the data will be saved to and loaded from.
        private int _hints;                         // The number of hints read from the data when the file is loaded.
        private bool _loaded;                       // Whether the data could be loaded or not.

        private static DataManager _instance;       // Instance of the DataManager.

        private Dictionary<string, string> _data;   // Dictionary that contains every level the player has finished:
                                                    // The key follows the structure "LevelCategory-LevelPack-LevelNumber", 
                                                    // And the value is a tuple of numbers "steps|perfect".
        
           
        /// <summary>
        /// Returns the instance of the DataManager.
        /// </summary>
        /// <returns>Instance of the DataManager.</returns>
        public static DataManager Instance()
        {
            return _instance;
        }
        
        /// <summary>
        /// Sets its initial values, and loads the data.
        /// </summary>
        private void Awake()
        {
            if (_instance == null)
            {
                _loaded = false;
                _instance = this;
                _path = Application.persistentDataPath + "/save.txt";
                Load();
                DontDestroyOnLoad(this.gameObject);
            }
            else Destroy(this);
        }

        /// <summary>
        /// If the file was loaded (or deleted, if the player cheated), the number of hints is set.
        /// </summary>
        private void Start()
        {
            if(_loaded) GameManager.Instance().SetClues(_hints);
        }
        
        /// <summary>
        /// When the player stops playing, the state is saved.
        /// </summary>
        private void OnApplicationQuit()
        {
            Save();
        }

        /// <summary>
        /// When the player stops playing temporally, the state is saved.
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus) Save();
        }
        
        
        // ----- PUBLIC METHODS ----- //
        
        /// <summary>
        /// Saves a new entry to the dictionary, or updates it if there was one before.
        /// </summary>
        /// <param name="levelData">Where the information of the level is saved.</param>
        /// <param name="steps">The number of steps the player needed to finish the level.</param>
        /// <param name="minimumSteps">The minimum number of steps needed to finish the level.</param>
        public void FinishLevel(LevelData levelData, int steps, int minimumSteps)
        {
            string key = GetKey(GameManager.Instance().GetCategoryName(levelData.CategoryNumber), levelData.PackNumber, levelData.LevelNumber);
            int previousSteps = -1;

            // If the player had played this level before, loads the number of steps it took them before.
            if(_data.ContainsKey(key))
            {
                string[] values = _data[key].Split('|');
                previousSteps = int.Parse(values[0]);
            }
            
            // If the player had never played this level, or they improved their steps, the new value is saved.
            if (previousSteps == -1 || previousSteps > steps)
            {
                string value = steps + "|";
                
                // If it was done in the minimum steps, saves 1 (a perfect score). Otherwise, it saves a 0 (not perfect).
                value += (steps == minimumSteps) ? 1 : 0;
                _data[key] = value;
                Save(); 
            }
        }
        
        /// <summary>
        /// Fills the number of steps and whether it was completed in the minimum number of steps or not.
        /// </summary>
        /// <param name="category">Category that is being loaded.</param>
        /// <param name="pack">Pack that is being loaded.</param>
        /// <param name="level">Level that is being loaded.</param>
        /// <param name="steps">The number of steps that the player used to solve the level (-1 if it has never been completed).</param>
        /// <param name="minimum">Whether the player finished the level in the minimum amount of steps possible (false if it has never been completed).</param>
        public void LoadLevel(string category, int pack, int level, out int steps, out bool minimum)
        {
            string key = GetKey(category, pack, level);

            //It is only contained when it's completed.
            if(_data.ContainsKey(key))
            {
                string[] value = _data[key].Split('|');
                steps =  int.Parse(value[0]);
                
                // 1 if the game was perfect; 0 otherwise.
                minimum = int.Parse(value[1]) == 1; 
            }
            else 
            {
                // -1 means a level has not been completed.
                steps = -1; 
                minimum = false;
            }
        }

        /// <summary>
        /// Returns the number of levels the player completed in a given pack of levels.
        /// </summary>
        /// <param name="categoryName">Levels category that is being checked.</param>
        /// <param name="levelPack">Levels pack that is being checked.</param>
        /// <returns></returns>
        public int GetPackCompletedLevels(string categoryName, int levelPack)
        {
            return _data.Count(e => e.Key.StartsWith(categoryName + "-" + levelPack + "-"));
        }
        
        
        // ----- PRIVATE ----- //
        
        /// <summary>
        /// Loads the state of the game, before the game starts.
        /// </summary>
        private void Load()
        {
            _data = new Dictionary<string, string>();
            
            // If it is the first time the player plays, there is no data stored.
            if (!File.Exists(_path)) return;

            string[] data = System.IO.File.ReadAllLines(_path);
            _loaded = true;
            if(!CompareSha256(data))
            {
                // If the hash is not the same (the player cheated), overwrites the whole file so that the progress get completely lost.
                System.IO.File.WriteAllLines(_path, new []{""});
                return;
            }

            // Reads each line, except the first one (which corresponds to the hash code).
            for (int i = 1; i < data.Length - 1; i++) 
            {
                var splits = data[i].Split(':');
                var formattedKey = splits[0].Split('-');
                
                // If it is the number of hints, it is saved for setting it later on.
                if (formattedKey.Length == 1) _hints = int.Parse(splits[1]);
                
                // If it is a level, saves the entry with its value.
                else if(formattedKey.Length == 3)
                {
                    _data[splits[0]] = splits[1];
                }
            }
        }
        
        /// <summary>
        /// Saves the current state of the game into a file.
        /// </summary>
        private void Save()
        {
            // Saves each of the levels that have been finished, as a pair "key:value".
            string textData = "";
            foreach (var pair in _data)
            {
                textData += pair.Key + ":" + pair.Value+"\n";
            }

            // Saves the number of clues the player has.
            textData += "clues:" + GameManager.Instance().GetHints();

            // Saves the hash code before anything else.
            string hash = ComputeSha256(textData);
            textData = hash + "\n" + textData;
            
            System.IO.File.WriteAllText(_path, textData);
        }
        
        /// <summary>
        /// Composes the key for an entry in the dictionary.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <param name="pack">Pack index (in the context of the category).</param>
        /// <param name="level">Level number (in the context of the pack).</param>
        /// <returns></returns>
        private string GetKey(string category, int pack, int level)
        {
            return $"{category}-{pack}-{level}";
        }
        
        /// <summary>
        /// Calculates the hash code before saving the file.
        /// </summary>
        /// <param name="data">Data to be saved.</param>
        /// <returns>The result of computing the data.</returns>
        private static string ComputeSha256(string data)
        {
            // Creates a SHA256.
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Converts the bytes to a string.
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Compares the hash saved in the data loaded to the one it should have.
        /// </summary>
        /// <param name="data">Data loaded from the save file.</param>
        /// <returns>true if both hash codes are the same; false otherwise.</returns>
        private static bool CompareSha256(string[] data)
        {
            string hash = data[0];
            
            // Ignores the first line: the hash code it had.
            List<string> list = data.ToList();
            list.RemoveAt(0);
            
            return hash == ComputeSha256(String.Join("\n", list));
        }
    }
}
