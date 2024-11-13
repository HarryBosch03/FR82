using System;
using System.Collections;
using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WorldButton))]
    public class ButtonAnimator : MonoBehaviour
    {
        public Transform rotor;
        public Vector3 idlePosition;
        public Vector3 idleRotation;
        public Vector3 pressedPosition;
        public Vector3 pressedRotation;
        public float lerpTime = 0.2f;
        public AnimationCurve lerpCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private float position;
        private WorldButton button;

        private void Awake()
        {
            button = GetComponent<WorldButton>();
        }

        private void Update()
        {
            position = Mathf.MoveTowards(position, button.value, Time.deltaTime / lerpTime);

            var t = lerpCurve.Evaluate(position);
            rotor.localPosition = Vector3.Lerp(idlePosition, pressedPosition, t);
            rotor.localRotation = Quaternion.Slerp(Quaternion.Euler(idleRotation), Quaternion.Euler(pressedRotation), t);
        }

        private void Reset()
        {
            rotor = transform.Find($"{name}.Rotor");
            if (rotor != null)
            {
                idlePosition = rotor.localPosition;
                idleRotation = rotor.localEulerAngles;

                pressedPosition = idlePosition;
                pressedRotation = idleRotation;
            }
        }
    }
}