using Configs.Impl;
using UnityEngine;
using Zenject;

namespace Tools
{
    public class FolovingCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        
        [Header("Target")]
        //public Transform target;
        [Inject] public CarPreset target;
        public float distance = 5f;
        public float minDistance = 2f;
        public float maxDistance = 12f;

        [Header("Rotation")]
        public float sensX = 150f;
        public float sensY = 120f;
        public float minY = -80f;
        public float maxY = 80f;

        [Header("Zoom")]
        public float zoomSpeed = 5f;

        [Header("Damping")]
        public float rotateDamp = 12f;
        public float zoomDamp = 12f;

        private float _yaw;
        private float _pitch;
        private float _currentDistance;

        private void Awake()
        {
            _camera = Camera.main;
        }
        
        void Start()
        {
            var targetPosition = target ? target.Car.transform.position : Vector3.zero;
            var direction = transform.position - targetPosition;
            _currentDistance = distance = direction.magnitude;
            
            var angles = transform.eulerAngles;
            _yaw = angles.y;
            _pitch = angles.x;
        }

        private void Update()
        {
            if (!target)
                return;

            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
                _pitch -= Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
                _pitch = Mathf.Clamp(_pitch, minY, maxY);
            }

            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance -= scroll * zoomSpeed * Mathf.Max(1f, distance * 0.25f);
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            _currentDistance = Mathf.Lerp(_currentDistance, distance, 1f - Mathf.Exp(-zoomDamp * Time.deltaTime));

            var rot = Quaternion.Euler(_pitch, _yaw, 0f);
            var pos = target.Car.transform.position - (rot * Vector3.forward * _currentDistance);

            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1f - Mathf.Exp(-rotateDamp * Time.deltaTime));
            transform.position = pos;

            transform.LookAt(target.Car.transform, Vector3.up);
        }
    }
}