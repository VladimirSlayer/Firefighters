using UnityEngine;
using Unity.Netcode;
using Features.UI;

namespace Features.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public Vector3 velocity;
        [HideInInspector] public bool inputEnabled = true;
        [SerializeField] private GameObject fpvCamera;

        private PlayerMovement movement;
        private PlayerLook look;

        public override void OnNetworkSpawn()
        {
            characterController = GetComponent<CharacterController>();
            movement = GetComponent<PlayerMovement>();
            look = GetComponent<PlayerLook>();

            if (!IsOwner)
            {
                if (fpvCamera != null)
                    fpvCamera.SetActive(false);
                return;
            }

            if (fpvCamera != null)
                fpvCamera.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            UIStateEvents.OnGameMenuToggled += HandleMenuToggle;
        }

        private void Update()
        {
            if (!IsOwner || characterController == null) return;

            if (inputEnabled)
            {
                look?.ProcessLook();
                movement?.ProcessMovement();
            }
            else
            {
                movement?.ApplyGravityOnly();
            }
        }

        private void HandleMenuToggle(bool isMenuOpen)
        {
            inputEnabled = !isMenuOpen;

            if (!isMenuOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public override void OnDestroy()
        {
            if (IsOwner)
                UIStateEvents.OnGameMenuToggled -= HandleMenuToggle;

            base.OnDestroy();
        }
    }
}
