using TMPro;
using UnityEngine;

namespace Utility
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FpsCounter : MonoBehaviour
    {
        private float[] _frameDeltaTimeArray;
        private int _lastFrameIndex;
        private TextMeshProUGUI _text;

        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _frameDeltaTimeArray = new float[50];
        }

        private void Update()
        {
            _frameDeltaTimeArray[_lastFrameIndex] = Time.deltaTime;
            _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;

            _text.text = $"fps {Mathf.RoundToInt(Calculate()).ToString()}";
        }

        private float Calculate()
        {
            float total = 0f;

            for (int i = 0; i < _frameDeltaTimeArray.Length; i++)
            {
                float delta = _frameDeltaTimeArray[i];
                total += delta;
            }

            return _frameDeltaTimeArray.Length / total;
        }
    }
}