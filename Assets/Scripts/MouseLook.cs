using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform Player;
    float xRottaion = 0f;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRottaion -= mouseY;
        xRottaion = Mathf.Clamp(xRottaion, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRottaion, 0f, 0f);
        Player.Rotate(Vector3.up * mouseX);
    }
}
