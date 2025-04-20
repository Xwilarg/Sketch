using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Sketch.Persistency
{
    public class PersistencyManager
    {
        // Exactly 16 bytes
        private const string _key = "Yuzu we love you";

        private Aes CreateAes()
        {
            var aes = Aes.Create();

            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.Mode = CipherMode.ECB;

            return aes;
        }

        private byte[] Encrypt(string s)
        {
            var aes = CreateAes();
            var encryptor = aes.CreateEncryptor();

            var data = Encoding.UTF8.GetBytes(s);
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        private string Decrypt(byte[] d)
        {
            var aes = CreateAes();
            var encryptor = aes.CreateDecryptor();

            var data = encryptor.TransformFinalBlock(d, 0, d.Length);
            return Encoding.UTF8.GetString(data);
        }

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "save.sav");

        private static PersistencyManager _instance;
        public static PersistencyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.Log($"[PER] Persistency Manager created, data will be saved at {SaveFilePath}");
                    _instance = new();
                }
                return _instance;
            }
        }

        private SaveData _saveData;
        public SaveData SaveData
        {
            get
            {
                if (_saveData == null)
                {
                    if (File.Exists(SaveFilePath))
                    {
                        _saveData = JsonConvert.DeserializeObject<SaveData>(Decrypt(File.ReadAllBytes(SaveFilePath)));
                        if (_saveData == null)
                        {
                            Debug.LogError("Save file couldn't be parsed, creating a new one...");
                            _saveData = new SaveData();
                        }
                    }
                    else
                    {
                        _saveData = new();
                    }
                }
                return _saveData;
            }
        }

        public void Save()
        {
            File.WriteAllBytes(SaveFilePath, Encrypt(JsonConvert.SerializeObject(_saveData)));
        }
    }
}