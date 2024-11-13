using System;
using System.Collections;
using UnityEngine;

namespace Runtime
{
    public class WorldButton : MonoBehaviour, IInteractable
    {
        public string displayTemplate = "Press Button";
        public ButtonType buttonType;
        public float tapTime = 0.2f;
        public float dragSensitivity = 1000f;

        public int valueMin;
        public int valueMax;
        public bool loop;

        private Vector2 lastLookPosition;

        public int value
        {
            get => Mathf.RoundToInt(partialValue);
            private set => partialValue = value;
        }
        
        public float partialValue { get; private set; }
        public float normalizedValue => Mathf.InverseLerp(valueMin, valueMax, partialValue);
        public bool state
        {
            get => value == 1;
            set => this.value = value ? 1 : 0;
        }
        public event Action<int> StateChangedEvent;

        private void ChangeValue(float newValue)
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                case ButtonType.DragSwitch:
                case ButtonType.ToggleSwitch:
                {
                    newValue = Mathf.Clamp01(newValue);
                    break;
                }
                case ButtonType.Dial:
                {
                    if (loop)
                    {
                        var r = valueMax - valueMin;
                        newValue = ((newValue - newValue) % r + r) % r + newValue;
                    }
                    else
                    {
                        newValue = Mathf.Clamp(newValue, valueMin, valueMax);
                    }
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            
            partialValue = newValue;
            StateChangedEvent?.Invoke(value);
        }

        public void Press(PlayerController invoker, Ray ray)
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                {
                    ChangeValue(1);
                    break;
                }
                case ButtonType.ToggleSwitch:
                {
                    ChangeValue(state ? 0 : 1);
                    break;
                }
                case ButtonType.DragSwitch:
                case ButtonType.Dial:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            lastLookPosition = GetPointOnPlane(ray);
        }

        public Vector2 GetPointOnPlane(Ray ray)
        {
            var plane = new Plane(transform.forward, transform.position);
            if (plane.Raycast(ray, out var enter))
            {
                return ray.GetPoint(enter);
            }
            return transform.position;
        }

        public void Drag(PlayerController invoker, Ray ray)
        {
            var point = GetPointOnPlane(ray);
            var delta = point - lastLookPosition;
            
            switch (buttonType)
            {
                case ButtonType.Button:
                case ButtonType.ToggleSwitch:
                {
                    break;
                }
                case ButtonType.DragSwitch:
                {
                    ChangeValue(partialValue + Vector3.Dot(delta, -transform.up) * dragSensitivity);
                    break;
                }
                case ButtonType.Dial:
                {
                    ChangeValue(partialValue + Vector3.Dot(delta, transform.right) * dragSensitivity);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
            
            
            lastLookPosition = point;
        }

        public void Release(PlayerController invoker)
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                {
                    ChangeValue(0);
                    break;
                }
                case ButtonType.DragSwitch:
                {
                    ChangeValue(state ? 1 : 0);
                    break;
                }
                case ButtonType.ToggleSwitch:
                case ButtonType.Dial:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void Increment(PlayerController invoker, float increment)
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                {
                    StartCoroutine(Tap());
                    break;
                }
                case ButtonType.DragSwitch:
                case ButtonType.ToggleSwitch:
                {
                    if (increment > 0) ChangeValue(0);
                    else ChangeValue(1);
                    break;
                }
                case ButtonType.Dial:
                {
                    ChangeValue(value + (increment > 0f ? 1 : -1));
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private IEnumerator Tap()
        {
            ChangeValue(1);
            yield return new WaitForSeconds(tapTime);
            ChangeValue(0);
        }

        public string GetInteractText()
        {
            var val = displayTemplate;
            return val;
        }

        public enum ButtonType
        {
            Button,
            ToggleSwitch,
            DragSwitch,
            Dial,
        }

        private void OnValidate()
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                case ButtonType.ToggleSwitch:
                case ButtonType.DragSwitch:
                {
                    valueMin = 0;
                    valueMax = 1;
                    loop = false;
                    break;
                }
                case ButtonType.Dial:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}