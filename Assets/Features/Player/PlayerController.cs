using UnityEngine;
using Unity.Netcode;
using Features.UI;

namespace Features.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float gravity = -9.81f;

        [Header("Mouse Look")]
        public float mouseSensitivity = 100f;
        public Transform cameraPivot;
        private float verticalRotation = 0f;

        private CharacterController controller;
        private Vector3 velocity;

        [SerializeField] private GameObject fpvCamera;

        private bool inputEnabled = true;

        public override void OnNetworkSpawn()
        {
            controller = GetComponent<CharacterController>();

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

        public override void OnDestroy()
        {
            if (IsOwner)
                UIStateEvents.OnGameMenuToggled -= HandleMenuToggle;

            base.OnDestroy();
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

        private void Update()
        {
            if (!IsOwner || controller == null)
            {
                return;
            }

            ApplyGravity();

            if (!inputEnabled)
            {
                controller.Move(velocity * Time.deltaTime);
                return;
            }

            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            transform.Rotate(Vector3.up * mouseX);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        private void HandleMovement()
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 move = transform.TransformDirection(input) * moveSpeed;

            if (controller.isGrounded)
            {
                if (velocity.y < 0) velocity.y = -2f;

                if (Input.GetButtonDown("Jump"))
                {
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                }
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move((move + velocity) * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
        }
    }
}
