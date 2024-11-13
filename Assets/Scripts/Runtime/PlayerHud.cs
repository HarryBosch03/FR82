using TMPro;
using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHud : MonoBehaviour
    {
        public TMP_Text conText;

        private PlayerController player;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        private void LateUpdate()
        {
            if (player.LookingAt != null)
            {
                conText.text = player.LookingAt.GetInteractText();
            }
            else
            {
                conText.text = string.Empty;
            }
        }
    }
}