using UnityEngine;
using Unity.Netcode;

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

        public override void OnNetworkSpawn()
        {
            controller = GetComponent<CharacterController>();

            // Важно: код запускаем ТОЛЬКО для локального игрока
            if (!IsOwner)
            {
                if (fpvCamera != null)
                    fpvCamera.SetActive(false);

                // Не отключай весь скрипт, иначе переменные (например velocity) не обрабатываются!
                return;
            }

            if (fpvCamera != null)
                fpvCamera.SetActive(true);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (!IsOwner || controller == null) return;

            HandleLook();
            HandleMovement();
        }

        void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            transform.Rotate(Vector3.up * mouseX);

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        void HandleMovement()
        {
            // WASD движение
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 move = transform.TransformDirection(input) * moveSpeed;

            // Прыжок
            if (controller.isGrounded)
            {
                if (velocity.y < 0) velocity.y = -2f; // стабильный контакт с землёй

                if (Input.GetButtonDown("Jump"))
                {
                    velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                }
            }

            // Применение гравитации
            velocity.y += gravity * Time.deltaTime;

            // Финальное движение
            controller.Move((move + velocity) * Time.deltaTime);
        }
    }
}
