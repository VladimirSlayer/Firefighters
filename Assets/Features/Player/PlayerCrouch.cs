using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerCrouch : MonoBehaviour
    {
        public float crouchHeight = 1f;
        public float crouchSpeedMultiplier = 0.5f;

        private float originalHeight;
        private bool isCrouching = false;

        private PlayerMovement movement;
        private CharacterController cc;
        private SystemActions input;

        private void Start()
        {
            movement = GetComponent<PlayerMovement>();
            cc = movement.controller.characterController;

            originalHeight = cc.height;

            input = new SystemActions();
            input.Player.Enable();

            input.Player.Crouch.performed += OnCrouch;
        }

        private void OnDestroy()
        {
            input.Player.Crouch.performed -= OnCrouch;
            input.Player.Disable();
        }

        private void OnCrouch(InputAction.CallbackContext ctx)
        {
            if (!movement.controller.inputEnabled)
                return;

            isCrouching = !isCrouching;

            cc.height = isCrouching ? crouchHeight : originalHeight;
        }

        private void Update()
        {
            if (!movement.controller.inputEnabled)
                return;

            if (isCrouching)
                movement.MultiplySpeed(crouchSpeedMultiplier);
        }
    }
}
