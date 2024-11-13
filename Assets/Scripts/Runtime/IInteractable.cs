using UnityEngine;

namespace Runtime
{
    public interface IInteractable
    {
        void Press(PlayerController invoker, Ray ray);
        void Drag(PlayerController invoker, Ray ray);
        void Release(PlayerController invoker);
        void Increment(PlayerController invoker, float increment);
        string GetInteractText();
    }
}
