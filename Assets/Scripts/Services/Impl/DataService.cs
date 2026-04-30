using System;
using System.IO;
using UnityEngine;

namespace Services.Impl
{
    public partial class DataService : IDataService
    {
        private readonly string _persistentPath;

        public DataService()
        {
            _persistentPath = Application.persistentDataPath;
        }

        protected void SaveJson<T>(string fileName, T data)
        {
            EnsureDirectory();
            var path = GetJsonPath(fileName);
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        protected T LoadJson<T>(string fileName, T fallback)
        {
            var path = GetJsonPath(fileName);
            if (!File.Exists(path))
                return fallback;

            var json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                return fallback;

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private float LoadFloat(string key, float fallback)
        {
            return PlayerPrefs.GetFloat(key, fallback);
        }

        private void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        private string GetJsonPath(string fileName)
        {
            return Path.Combine(_persistentPath, fileName);
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(_persistentPath))
                Directory.CreateDirectory(_persistentPath);
        }
    }
}