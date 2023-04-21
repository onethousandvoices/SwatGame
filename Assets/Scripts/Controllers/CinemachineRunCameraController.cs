using Cinemachine;
using NTC.Global.Cache;
using SWAT.Events;
using System.Collections;
using UnityEngine;

namespace Controllers
{
    public class CinemachineRunCameraController : MonoCache
    {
        [SerializeField] private CinemachineVirtualCamera _camera;
        [SerializeField, Range(0.1f, 4f)] private float _speed0to1;
        [SerializeField, Range(0.1f, 4f)] private float _speed1to2;
        [SerializeField, Range(0.1f, 4f)] private float _speed2to3;
        [SerializeField, Range(0.1f, 4f)] private float _speed3to4;
        [SerializeField, Range(0.1f, 4f)] private float _speed4to5;

        private CinemachineTrackedDolly _dolly;
        private float[] _speeds;
        private int _index;

        protected override void OnEnabled()
        {
            _index = -1;
            _speeds = new float[5];
            _speeds[0] = _speed0to1;
            _speeds[1] = _speed1to2;
            _speeds[2] = _speed2to3;
            _speeds[3] = _speed3to4;
            _speeds[4] = _speed4to5;

            _dolly = _camera.GetCinemachineComponent<CinemachineTrackedDolly>();
            _dolly.m_PathPosition = 0f;
            
            GameEvents.Register<Event_GameStart>(OnStart);
            GameEvents.Register<Event_PlayerRunStarted>(OnPlayerRun);
        }

        private void OnStart(Event_GameStart obj)
            => _dolly.m_PathPosition = 0f;

        private void OnPlayerRun(Event_PlayerRunStarted obj)
            => StartCoroutine(LerpToNextPoint());

        private IEnumerator LerpToNextPoint()
        {
            _index++;
            float startPoint = _dolly.m_PathPosition;
            float target = startPoint + 1;
            float speed = _index < _speeds.Length ? _speeds[_index] : 1f;

            float t = 0f;
            while (t < 1f)
            {
                _dolly.m_PathPosition = Mathf.Lerp(startPoint, target, t);
                t += Time.deltaTime * speed;
                yield return null;
            }
            _dolly.m_PathPosition = target;
        }
    }
}