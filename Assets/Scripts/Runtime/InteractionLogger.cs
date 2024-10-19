using UnityEngine;

namespace Runtime
{
    public class InteractionLogger : MonoBehaviour, IInteractable
    {
        public void StartInteract(GameObject interactor) => Debug.Log($"{interactor.name} started interacting with {name}");
        public void StopInteract(GameObject interactor) => Debug.Log($"{interactor.name} stopped interacting with {name}");
    }
}