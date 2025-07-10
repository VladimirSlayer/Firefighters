using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerSprint : MonoBehaviour
    {
        public float sprintMultiplier = 1.5f;

        private PlayerMovement movement;
        private SystemActions input;
        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            input = new SystemActions();
            input.Player.Enable();
        }

        private void Update()
        {
            if (!movement.controller.inputEnabled)
                return;

            if (input.Player.Sprint.IsPressed())
                movement.MultiplySpeed(sprintMultiplier);
        }
    }
}
