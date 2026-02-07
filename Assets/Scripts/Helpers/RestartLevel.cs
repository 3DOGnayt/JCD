using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Helpers
{
    public class RestartLevel : MonoBehaviour
    {
        public Button RestartButton;

        private void Awake()
        {
            RestartButton.onClick.AddListener(Restart);
        }

        private void Restart()
        {
            SceneManager.LoadScene(0);
        }
    }
}