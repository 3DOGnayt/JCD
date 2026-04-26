using UnityEngine;

namespace uk.vroad.urvr
{
    public class UWorldSpinY : MonoBehaviour
    {
        public float speed = -12f; // set to -12 for earth, +16 for SunLight 

        private Vector3 eulers = new Vector3(0, 0, 0);

        void Update()
        {
            eulers.y += speed * Time.deltaTime / Time.timeScale;

            transform.eulerAngles = eulers;
        }
    }
}
