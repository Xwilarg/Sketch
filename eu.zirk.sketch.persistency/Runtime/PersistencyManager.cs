using Newtonsoft.Json;
using System.IO;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Sketch.Persistency
{
#if UNITY_WEBGL && !UNITY_EDITOR
    internal static class WebGLSyncFiles
    {
        [DllImport("__Internal")]
        internal static extern void SyncFiles();
    }
#endif

    public class PersistencyManager<T>
        where T : ISaveData, new()
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

        private static PersistencyManager<T> _instance;
        public static PersistencyManager<T> Instance
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

        public int PersistencySize => File.Exists(SaveFilePath) ? File.ReadAllBytes(SaveFilePath).Length : 0;

        private T _saveData;
        public T SaveData
        {
            get
            {
                if (_saveData == null)
                {
                    ReloadSaves();
                }
                return _saveData;
            }
        }

        public void Save()
        {
            File.WriteAllBytes(SaveFilePath, Encrypt(JsonConvert.SerializeObject(_saveData)));
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLSyncFiles.SyncFiles();
#endif
        }

        public void DeleteSave()
        {
            if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            _saveData = new();
        }

        public void ReloadSaves()
        {
            if (File.Exists(SaveFilePath))
            {
                _saveData = JsonConvert.DeserializeObject<T>(Decrypt(File.ReadAllBytes(SaveFilePath)));
                if (_saveData == null)
                {
                    Debug.LogError("Save file couldn't be parsed, creating a new one...");
                    _saveData = new();
                }
            }
            else
            {
                _saveData = new();
            }
        }
    }
}