using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Remedy.Framework;

namespace Remedy.Framework
{
    public class ScriptableLog : SingletonData<ScriptableLog>
    {
        public List<string> logs = new List<string>();

        public static void AddLog(string message)
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timeStamp}] {message}";
            Instance.logs.Add(logMessage);
        }

        public void SaveToFile()
        {
            string logFilePath = Path.Combine(Application.persistentDataPath, "log.txt");
            File.WriteAllLines(logFilePath, logs);
        }
    }
}