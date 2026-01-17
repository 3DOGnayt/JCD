using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _menuPanel;

        private void Awake()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(CloseSettings);
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(CloseSettings);
        }

        private void CloseSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_menuPanel != null)
                _menuPanel.SetActive(true);
        }
    }
}
