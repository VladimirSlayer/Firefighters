using UnityEngine;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerMovement))]
    public class PlayerSprint : MonoBehaviour
    {
        public float sprintMultiplier = 1.5f;
        private PlayerMovement movement;
        private PlayerController pc;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            pc = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (pc.carriedLoad > 0f) return;

            if (movement != null && Input.GetKey(KeyCode.LeftShift))
                movement.MultiplySpeed(sprintMultiplier);
        }
    }
}
