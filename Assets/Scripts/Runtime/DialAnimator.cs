using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WorldButton))]
    public class DialAnimator : MonoBehaviour
    {
        public Transform rotor;
        public Vector3 baseRotation;
        public Vector3 rotationAxis = Vector3.forward;
        public float angleRange;
        public float lerpTime = 0.2f;
        
        private float position;
        private WorldButton button;

        private void Awake()
        {
            button = GetComponent<WorldButton>();
        }

        private void Update()
        {
            position = Mathf.Lerp(position, button.normalizedValue, Time.deltaTime / lerpTime);

            rotor.localRotation = Quaternion.Euler(baseRotation) * Quaternion.AngleAxis(position * angleRange, rotationAxis);
        }

        private void Reset()
        {
            rotor = transform.Find($"{name}.Rotor");
            if (rotor != null)
            {
                baseRotation = rotor.localEulerAngles;
            }
        }
    }
}