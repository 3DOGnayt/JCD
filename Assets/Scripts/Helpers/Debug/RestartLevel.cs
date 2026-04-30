using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Helpers.Debug
{
    public class RestartLevel : MonoBehaviour
    {
        [SerializeField] private Button _restartButton;

        private void Awake()
        {
            _restartButton.onClick.AddListener(Restart);
        }

        private void Restart()
        {
            SceneManager.LoadScene(0);
        }
    }
}