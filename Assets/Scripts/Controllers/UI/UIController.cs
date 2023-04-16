using NTC.Global.Cache;
using SWAT.Events;
using UnityEngine;

namespace Controllers
{
    public class UIController : MonoCache
    {
        [SerializeField] private Animator _animator;
        
        protected override void OnEnabled()
        {
            GameEvents.Register<Event_PlayerKilled>(PlayerKilled);
            GameEvents.Register<Event_LevelCompleted>(LevelCompleted);
        }

        private void LevelCompleted(Event_LevelCompleted obj)
        {
        }

        private void PlayerKilled(Event_PlayerKilled obj)
        {
        }
    }
}