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
        private Dictionary<string, int> _data;
        private int clues;
        
        private static DataManager _instance;
        public void FinishLevel(string categoryName, int levelPack, int levelNum, int steps)
        {
            string key = GetKey(categoryName, levelPack, levelNum);
            if (!_data.ContainsKey(key) || _data[key] > steps) _data[key] = steps;
        }
        
        // cat - pack - levNum: valor
        // clues: valor
        private void Awake()
        {
            if (_instance == null)
            {
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
            Debug.Log("guardamos");
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
            
            _data = new Dictionary<string, int>();
            if (File.Exists(_path))
            {
                string[] data = System.IO.File.ReadAllLines(_path);
                if(CompareSha256(data)) Debug.Log("Todo bien");
                else
                {
                    Debug.LogError("Mas engañao");
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
                        _data[splits[0]] = int.Parse(splits[1]);
                    }
                }
            }
        }

        private void Start()
        {
            GameManager.Instance().SetClues(clues);
        }

        public void LoadLevel(string categoryName, int levelPack, int levelNum, out int steps)
        {
            string key = GetKey(categoryName, levelPack, levelNum);
            if (_data.ContainsKey(key)) steps = _data[key];
            else steps = -1;
        }
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
