using UnityEngine;
using UnityEngine.UI;

namespace Utility
{
    [RequireComponent(typeof(Text))]
    public class Logger : MonoBehaviour
    {
        private Text _text;

        private void OnEnable()
        {
            _text                          =  GetComponent<Text>();
            Application.logMessageReceived += Log;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        private void Log(string condition, string stacktrace, LogType type)
        {
            if (_text.text.Length > 1500) return;

            if (type == LogType.Error || type == LogType.Exception)
            {
                _text.text += $"\n{condition}\n{stacktrace}";
            }
        }
    }
}