using UnityEngine;
using UnityEngine.UI;

namespace Helpers.Debug
{
    public class Debug_StateToggle : MonoBehaviour
    {
        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _target;

        private void OnEnable()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.AddListener(ToggleTarget);
        }

        private void OnDisable()
        {
            if (_toggleButton != null)
                _toggleButton.onClick.RemoveListener(ToggleTarget);
        }

        private void ToggleTarget()
        {
            if (_target == null)
                return;

            _target.SetActive(!_target.activeSelf);
        }
    }
}