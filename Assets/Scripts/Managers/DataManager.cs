using System;
using System.Collections;
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
        private static string _path;
        private Dictionary<string, string> _data;
        private int clues;
        private bool _loaded;
        
        private static DataManager _instance;

        // TODO que reciba un levelData? Idunno this may work
        public void FinishLevel(LevelData levelData, int steps, int minimumSteps)
        {
            string key = GetKey(GameManager.Instance().GetCategoryName(levelData.CategoryNumber), levelData.PackNumber, levelData.LevelNumber);
            
            int previousSteps = -1;
            if(_data.ContainsKey(key))
            {
                string[] values = _data[key].Split('|');
                previousSteps = int.Parse(values[0]);
            }

            if (previousSteps == -1 || previousSteps > steps)
            {
                string value = steps + "|";
                value += (steps == minimumSteps) ? 1 : 0;
                _data[key] = value;
                Save(); 
            }
        }
        
        // cat - pack - levNum: valor
        // clues: valor
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

        public static DataManager Instance()
        {
            return _instance;
        }
        public void Save()
        {
            string textData = "";
            foreach (var pair in _data)
            {
                textData += pair.Key + ":" + pair.Value+"\n";
            }

            
            textData += "clues:" + GameManager.Instance().GetHints();

            string hash = ComputeSha256(textData);
            textData = hash + "\n" + textData;
            
            System.IO.File.WriteAllText(_path, textData);
        }

        public void Load()
        {
            _data = new Dictionary<string, string>();
            if (File.Exists(_path))
            {
                _loaded = true;
                
                string[] data = System.IO.File.ReadAllLines(_path);
                if(!CompareSha256(data))
                {
                    // If the hash is not the same (the player cheated), overwrites the whole file so that the progress get completely lost.
                    System.IO.File.WriteAllLines(_path, new []{""});
                    return;
                }

                foreach (var line in data)
                {
                    var splits = line.Split(':');
                    var formattedKey = splits[0].Split('-');

                    if(splits.Length == 1) continue;
                    if (formattedKey.Length == 1) clues = int.Parse(splits[1]);
                    else if(formattedKey.Length == 3)
                    {
                        _data[splits[0]] = splits[1];
                    }
                }
            }
        }

        private void Start()
        {
            // If the file was loaded (or deleted, if the player cheated), the number of hints is set.
            if(_loaded) GameManager.Instance().SetClues(clues);
        }

        //With the given LevelData fills the number of steps and whether it was completed in the minimum number
        //of steps or not. On uncompleted level steps will be -1
        public void LoadLevel(string category, int pack, int level, out int steps, out bool minimum)
        {
            string key = GetKey(category, pack, level);


            //It is only contained when it's completed
            if(_data.ContainsKey(key))
            {
                string[] value = _data[key].Split('|');
                steps =  int.Parse(value[0]);
                minimum = int.Parse(value[1]) == 1; //1 on perfect game 0 otherwise 
            }
            else 
            {
                minimum = false;
                steps = -1; //We signal this way an uncompleted level
            }
        }

        //Composes the key for a level in the dictionary
        private string GetKey(string name, int pack, int level)
        {
            return $"{name}-{pack}-{level}";
        }
        
        private static string ComputeSha256(string data)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static bool CompareSha256(string[] data)
        {
            string hash = data[0];
            List<string> list = data.ToList();
            list.RemoveAt(0);
            string hashCheck = ComputeSha256(String.Join("\n", list));
            
            return hash == hashCheck;
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if(pauseStatus) Save();
        }

        public int GetPackCompletedLevels(string categoryName, int levelPack)
        {
            return _data.Count(e => e.Key.StartsWith(categoryName + "-" + levelPack + "-"));
        }
    }
}
