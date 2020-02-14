using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMouseLook : MonoBehaviour
{
    // Class References //

    // Public Variables //
    public float mouseSensitivity = 100f;

    public Transform playerBody;

    // Private Variables //
    // Input Rotation
    private float mouseX;
    private float xRotation = 0f;
    private float mouseY;

    // Start is called before the first frame update
    void Start()
    {
        // removes and locks mouse to the centre of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerRotation();
    }

    // Handles Player Rotation
    private void PlayerRotation()
    {
        //if controller is connected, change input axis? this should be already handled by InputManager in Unity
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; //if this is positive, affect is flipped
        //ensure rotation around x axis does not exceed these
        //values to avoid inverse rotation and gimble lock
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

}
