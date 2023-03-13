using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SWAT.Utility
{
    public abstract class GDPDownloader
    {
        private const string _fileName = "BasicEntityCfg";
        private const string _uri      = "https://docs.google.com/spreadsheets/d/11kXljn6Jm78n6UL-Y7KuIKvCZVRIqHqqkapolvSeTF4/export?format=csv";

        private static string _filePath => Application.dataPath + "/Resources/BasicEntityCfg.csv";

        [MenuItem("Tools/DownloadGDP")]
        private static async void DownloadGdp()
        {
            Object cfg = Resources.Load(_fileName);

            if (cfg != null)
            {
                Debug.LogError("Config exists. Deleting...");
                File.Delete(_filePath);
            }

            WebClient downloader = new WebClient();
            await downloader.DownloadFileTaskAsync(new Uri(_uri), _filePath);

            Debug.LogError("Download completed!");
        }
    }
}