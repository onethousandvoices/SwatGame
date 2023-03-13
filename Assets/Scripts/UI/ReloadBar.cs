using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ReloadBar : MonoCache
    {
        [SerializeField] private GameObject _holder;
        [SerializeField] private Image      _filler;

        protected override void OnEnabled()
        {
            DisableBar();
        }

        public void EnableBar()
        {
            _holder.SetActive(true);
        }

        public void SetProgression(float progress)
        {
            _filler.fillAmount = progress;
        }

        public void DisableBar()
        {
            _holder.SetActive(false);
        }
    }
}