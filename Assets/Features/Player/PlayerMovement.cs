using UnityEngine;
using Features.Player;

[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    private float currentSpeedMultiplier = 1f;

    public PlayerController controller { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    public void ProcessMovement()
    {
        var cc = controller.characterController;

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 move = transform.TransformDirection(input) * walkSpeed * currentSpeedMultiplier;

        if (cc.isGrounded)
        {
            if (controller.velocity.y < 0) controller.velocity.y = -2f;

            if (Input.GetButtonDown("Jump"))
            {
                controller.velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }
        }

        controller.velocity.y += gravity * Time.deltaTime;
        cc.Move((move + controller.velocity) * Time.deltaTime);

        currentSpeedMultiplier = 1f;
    }

    public void MultiplySpeed(float multiplier)
    {
        currentSpeedMultiplier *= multiplier;
    }

    public void ApplyGravityOnly()
    {
        var cc = controller.characterController;

        if (cc.isGrounded && controller.velocity.y < 0)
        {
            controller.velocity.y = -2f;
        }

        controller.velocity.y += gravity * Time.deltaTime;
        cc.Move(controller.velocity * Time.deltaTime);
    }
}

