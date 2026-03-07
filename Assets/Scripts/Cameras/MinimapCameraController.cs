using UnityEngine;

namespace Cameras
{
    public sealed class MinimapCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera != null ? _camera : GetComponent<Camera>();
    }
}