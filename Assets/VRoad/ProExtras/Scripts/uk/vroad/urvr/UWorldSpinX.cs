using System;
using UnityEngine;

namespace uk.vroad.urvr
{
    public class UWorldSpinX : MonoBehaviour
    {
        public float speed = 7f; // set to 7 for earth holder

        private Vector3 eulers = new Vector3(0, 0, 0);
        private double rotationY;

        private const float SHIFT_NS = 15f;
        private const float MAX_NS = 45f;
        private const double DEG2RAD = (float) Math.PI / 180f;

        void Update()
        {
            rotationY += speed * Time.deltaTime / Time.timeScale;

            eulers.x = SHIFT_NS + (MAX_NS * (float) Math.Sin(DEG2RAD * rotationY));

            transform.eulerAngles = eulers;
        }
    }
}
