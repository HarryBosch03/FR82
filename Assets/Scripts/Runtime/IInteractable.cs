using UnityEngine;

namespace Runtime
{
    public interface IInteractable
    {
        void StartInteract(GameObject interactor);
        void StopInteract(GameObject interactor);
    }
}