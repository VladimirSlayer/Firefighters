using UnityEngine;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovement : MonoBehaviour
    {
        public float walkSpeed = 5f;
        public float gravity = -9.81f;
        public float jumpForce = 5f;

        private CharacterController cc;
        private PlayerController pc;
        private float currentSpeedMultiplier = 1f;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            pc = GetComponent<PlayerController>();
        }

        public void ProcessMovement()
        {
            // если несём нагрузку — скорость 50%
            float speedFactor = pc.carriedLoad > 0f ? 0.5f : 1f;
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            Vector3 move = transform.TransformDirection(input) * walkSpeed * speedFactor * currentSpeedMultiplier;

            if (cc.isGrounded)
            {
                if (pc.velocity.y < 0f) pc.velocity.y = -2f;
                if (Input.GetButtonDown("Jump"))
                    pc.velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            pc.velocity.y += gravity * Time.deltaTime;
            cc.Move((move + pc.velocity) * Time.deltaTime);
            currentSpeedMultiplier = 1f;
        }

        public void MultiplySpeed(float multiplier)
        {
            currentSpeedMultiplier *= multiplier;
        }

        public void ApplyGravityOnly()
        {
            if (cc.isGrounded && pc.velocity.y < 0f) pc.velocity.y = -2f;
            pc.velocity.y += gravity * Time.deltaTime;
            cc.Move(pc.velocity * Time.deltaTime);
        }
    }
}
