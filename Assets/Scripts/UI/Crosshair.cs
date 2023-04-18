using Controllers;
using NTC.Global.Cache;
using SWAT.Behaviour;
using SWAT.Events;
using UnityEngine;
using UnityEngine.UI;

namespace SWAT
{
    public class Crosshair : MonoCache, ITarget
    {
        [SerializeField, Range(1f, 30f)] private float _speed;
        [SerializeField] private RectTransform _barHolder;
        [SerializeField] private Image _barFiller;
        [SerializeField] private Image _crosshairFiller;
        [SerializeField] private Image _crosshairImage;
        [SerializeField] private float _imageWidth;
        [SerializeField] private float _imageHeight;

        private Camera _camera;
        private TutorialController _tutorialController;
        private RectTransform _rect;

        private Vector3 _startDragMouse;
        private Vector3 _endDragMouse;
        private Vector3 _startDragPos;
        private Vector3 _endDragPos;
        private Vector3 _delta;

        private int _obstacleLayer;

        protected override void OnEnabled()
        {
            _camera = ObjectHolder.GetObject<Camera>();
            _tutorialController = ObjectHolder.GetObject<TutorialController>();
            _obstacleLayer = LayerMask.GetMask("Obstacle");

            _rect = Get<RectTransform>();

            GameEvents.Register<Event_PlayerChangedPosition>(_ => Enable());
            GameEvents.Register<Event_PlayerRunStarted>(_ => Disable());
            GameEvents.Register<Event_CivilianLookEnded>(_ => Enable());

            ReloadReady();
        }

        protected override void Run()
        {
            if (!_tutorialController.IsInputAllowed)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                _startDragMouse = Input.mousePosition;
                _startDragPos = transform.position;
            }

            if (Input.GetMouseButton(0))
            {
                if (_startDragPos == Vector3.zero)
                    return;

                _endDragMouse = Input.mousePosition;
                _delta = _endDragMouse - _startDragMouse;

                if (_delta.sqrMagnitude < 5)
                    return;

                GameEvents.Call(new Event_CrosshairMoved());

                _endDragPos = new Vector3(
                    _startDragPos.x + _delta.x,
                    _startDragPos.y + _delta.y);

                transform.position = Vector3.Lerp(transform.position, _endDragPos, Time.deltaTime * _speed);

                Vector3 clampPoint = _camera.ScreenToViewportPoint(transform.position);

                clampPoint.x = Mathf.Clamp(clampPoint.x, _imageWidth, 1 - _imageWidth);
                clampPoint.y = Mathf.Clamp(clampPoint.y, _imageHeight, 1 - _imageHeight);

                transform.position = _camera.ViewportToScreenPoint(clampPoint);
            }

            else
                _endDragPos = Vector3.zero;
        }

        private Vector3 RayHit()
        {
            Ray ray = _camera.ScreenPointToRay(_crosshairImage.transform.position);
            Physics.Raycast(ray, out RaycastHit hit, 100f, _obstacleLayer);
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta);
#endif
            return hit.point;
        }

        public void EnableBar()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _barHolder.transform.position = _crosshairImage.transform.position;

            _barHolder.gameObject.SetActive(true);
            _crosshairImage.gameObject.SetActive(false);
            _crosshairFiller.gameObject.SetActive(false);
        }

        public void ReloadReady()
        {
            _crosshairImage.transform.position = _barHolder.transform.position;
            _crosshairFiller.transform.position = _barHolder.transform.position;

            _barHolder.gameObject.SetActive(false);
            _crosshairImage.gameObject.SetActive(true);
            _crosshairFiller.gameObject.SetActive(true);
            _crosshairFiller.fillAmount = 1f;
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }

        private void Enable()
        {
            gameObject.SetActive(true);
            _rect.anchoredPosition = Vector3.zero;
        }

        public Vector3 GetTarget() => RayHit();
        public void SetReloadProgression(float progress) => _barFiller.fillAmount = progress;
        public void SetCrosshairProgression(float progress) => _crosshairFiller.fillAmount = progress;
    }
}