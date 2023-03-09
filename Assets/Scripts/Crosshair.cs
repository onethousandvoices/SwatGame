using Controllers;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace SWAT
{
    public class Crosshair : MonoCache
    {
        [SerializeField, Range(1f, 30f)] private float _speed;

        private Image _image;

        private Vector3 _startDragMouse;
        private Vector3 _endDragMouse;
        private Vector3 _startDragPos;
        private Vector3 _endDragPos;
        private Vector3 _difference;

        private Camera _camera;

        private float _imageWidth;
        private float _imageHeight;
        private int   _obstacleLayer;

        protected override void OnEnabled()
        {
            _camera = ObjectHolder.GetObject<Camera>();
            _image  = ChildrenGet<Image>();

            _imageWidth  = _image.rectTransform.rect.width  / 2;
            _imageHeight = _image.rectTransform.rect.height / 2;

            _obstacleLayer = LayerMask.GetMask("Obstacle");
        }

        protected override void Run()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startDragMouse = Input.mousePosition;
                _startDragPos   = transform.position;
            }

            if (Input.GetMouseButton(0))
            {
                _endDragMouse = Input.mousePosition;
                _difference   = _endDragMouse - _startDragMouse;

                if (_difference.sqrMagnitude < 5) return;

                _endDragPos = new Vector3(
                    _startDragPos.x + _difference.x,
                    _startDragPos.y + _difference.y);

                transform.position = Vector3.Lerp(transform.position, _endDragPos, Time.deltaTime * _speed);

                Vector3 clampedPos = _image.transform.position;

                clampedPos.x = Mathf.Clamp(clampedPos.x, _imageWidth,  Screen.width  - _imageWidth);
                clampedPos.y = Mathf.Clamp(clampedPos.y, _imageHeight, Screen.height - _imageHeight);

                _image.transform.position = clampedPos;
            }

            else
            {
                _endDragPos = Vector3.zero;
            }
        }

        public Vector3 RayHit()
        {
            Ray ray = _camera.ScreenPointToRay(_image.transform.position);
            Physics.Raycast(ray, out RaycastHit hit, 100f, _obstacleLayer);
#if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.magenta);
#endif
            return hit.point;
        }
    }
}