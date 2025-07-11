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
        [HideInInspector] public float carriedLoad = 0f;

        [SerializeField] private GameObject fpvCamera;

        [SerializeField] private Inventory inventory;
        [SerializeField] private GameObject inventoryUIRoot;
        private PlayerMovement movement;
        private PlayerLook look;
        private PlayerSprint sprint;

        public override void OnNetworkSpawn()
        {
            characterController = GetComponent<CharacterController>();
            movement = GetComponent<PlayerMovement>();
            look = GetComponent<PlayerLook>();
            sprint = GetComponent<PlayerSprint>();

            if (!IsOwner)
            {
                if (fpvCamera != null) fpvCamera.SetActive(false);
                inventoryUIRoot?.SetActive(false);
                return;
            }
            if (fpvCamera != null) fpvCamera.SetActive(true);
            inventoryUIRoot?.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            UIStateEvents.OnGameMenuToggled += HandleMenuToggle;

            if (inventory != null)
            {
                inventory.InitUI();
            }
            else
            {
                Debug.LogWarning("Inventory не присвоен в инспекторе!");
            }
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
            if (IsOwner) UIStateEvents.OnGameMenuToggled -= HandleMenuToggle;
            base.OnDestroy();
        }

        public void SetCarryLoad(float kg) => carriedLoad = kg;
    }
}
