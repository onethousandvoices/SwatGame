using Controllers;
using NTC.Global.Cache;
using SWAT.Behaviour;
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

        private GameObject _currentImage;
        private Vector3 _startDragMouse;
        private Vector3 _endDragMouse;
        private Vector3 _startDragPos;
        private Vector3 _endDragPos;
        private Vector3 _delta;

        private Camera _camera;

        private float _imageWidth;
        private float _imageHeight;
        private int _obstacleLayer;

        protected override void OnEnabled()
        {
            _camera = ObjectHolder.GetObject<Camera>();

            _obstacleLayer = LayerMask.GetMask("Obstacle");

            DisableBar();
        }

        protected override void Run()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startDragMouse = Input.mousePosition;
                _startDragPos = transform.position;
            }

            if (Input.GetMouseButton(0))
            {
                _endDragMouse = Input.mousePosition;
                _delta = _endDragMouse - _startDragMouse;

                if (_delta.sqrMagnitude < 5) return;

                _endDragPos = new Vector3(
                    _startDragPos.x + _delta.x,
                    _startDragPos.y + _delta.y);

                transform.position = Vector3.Lerp(transform.position, _endDragPos, Time.deltaTime * _speed);

                Vector3 clampedPos = _currentImage.transform.position;

                clampedPos.x = Mathf.Clamp(clampedPos.x, _imageWidth, Screen.width - _imageWidth);
                clampedPos.y = Mathf.Clamp(clampedPos.y, _imageHeight, Screen.height - _imageHeight);

                _currentImage.transform.position = clampedPos;
            }

            else
            {
                _endDragPos = Vector3.zero;
            }
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
            _barHolder.transform.position = _crosshairImage.transform.position;

            _currentImage = _barHolder.gameObject;
            _barHolder.gameObject.SetActive(true);
            _crosshairImage.gameObject.SetActive(false);
            _crosshairFiller.gameObject.SetActive(false);
            SetWidthAndHeight(false);
        }

        public void SetReloadProgression(float progress)
        {
            _barFiller.fillAmount = progress;
        }

        public void SetCrosshairProgression(float progress)
        {
            _crosshairFiller.fillAmount = progress;
        }

        public void DisableBar()
        {
            _crosshairImage.transform.position = _barHolder.transform.position;
            _crosshairFiller.transform.position = _barHolder.transform.position;

            _currentImage = _crosshairImage.gameObject;
            _barHolder.gameObject.SetActive(false);
            _crosshairImage.gameObject.SetActive(true);
            _crosshairFiller.gameObject.SetActive(true);
            _crosshairFiller.fillAmount = 1f;
            SetWidthAndHeight(true);
        }

        private void SetWidthAndHeight(bool isCrosshair)
        {
            if (isCrosshair)
            {
                _imageWidth = _crosshairImage.rectTransform.rect.width / 2;
                _imageHeight = _crosshairImage.rectTransform.rect.height / 2;
            }
            else
            {
                _imageWidth = _barFiller.rectTransform.rect.width / 2;
                _imageHeight = _barFiller.rectTransform.rect.height / 2;
            }
        }

        public Vector3 GetTarget()
        {
            return RayHit();
        }
    }
}