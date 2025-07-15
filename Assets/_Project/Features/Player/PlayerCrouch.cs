using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCrouch : MonoBehaviour
    {
        [Header("Настройки приседания")]
        public float crouchHeight         = 1f;
        public float crouchSpeedMultiplier = 0.5f;

        private float                  originalHeight;
        private bool                   isCrouching;
        private PlayerMovement         movement;
        private PlayerController       pc;
        private CharacterController    cc;
        private SystemActions          input;

        private void Awake()
        {
            movement       = GetComponent<PlayerMovement>();
            pc             = GetComponent<PlayerController>();
            cc             = GetComponent<CharacterController>();
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
            if (!pc.inputEnabled)
                return;

            isCrouching = !isCrouching;
            cc.height   = isCrouching ? crouchHeight : originalHeight;
        }

        private void Update()
        {
            if (!pc.inputEnabled)
                return;

            if (isCrouching)
                movement.MultiplySpeed(crouchSpeedMultiplier);
        }
    }
}
