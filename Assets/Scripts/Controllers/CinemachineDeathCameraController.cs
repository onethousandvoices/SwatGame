using Cinemachine;
using NTC.Global.Cache;
using SWAT.Events;
using UnityEngine;

namespace Controllers
{
    public class CinemachineDeathCameraController : MonoCache
    {
        [SerializeField] private CinemachineStateDrivenCamera _stateDriven;
        [SerializeField] private CinemachineVirtualCamera _deathCamera;

        protected override void OnEnabled()
        {
            GameEvents.Register<Event_GameStart>(OnGameStart);
            GameEvents.Register<Event_GameOver>(OnGameOver);
        }

        private void OnGameOver(Event_GameOver obj)
        {
            _deathCamera.Priority = 666;
        }

        private void OnGameStart(Event_GameStart obj)
        {
            _deathCamera.Priority = 10;
            _stateDriven.Priority = 100;
        }
    }
}