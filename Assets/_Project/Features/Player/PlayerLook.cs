using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform cameraPivot;

    private float verticalRotation = 0f;

    public void ProcessLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}

