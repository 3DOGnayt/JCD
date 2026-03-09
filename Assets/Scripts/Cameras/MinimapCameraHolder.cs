using UnityEngine;

namespace Cameras
{
    public sealed class MinimapCameraHolder : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera != null ? _camera : GetComponent<Camera>();
    }
}