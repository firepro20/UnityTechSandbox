using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance { get { return instance; } }

    // Class References //
    public CharacterController characterController;


    // Public Variables //
    // Player Movement
    public float playerSpeed = 10f;
    public float mouseSensitivity = 100f;
    //public Transform playerBody;

    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    // Input Rotation
    private float mouseX;
    private float xRotation = 0f;
    private float mouseY;
    // Movement Flags
    private bool toggleDash;
    private bool isGrounded; 

    void Awake()
    {
        /* Singleton */
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
        /* End of Singleton */
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInputDevices();
        PlayerMovement();
        PlayerRotation();
    }

    // Handles player movement
    private void PlayerMovement()
    {
        xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * xAxis + transform.forward * zAxis;
        characterController.Move(movement * playerSpeed * Time.deltaTime);
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
        transform.Rotate(Vector3.up * mouseX);
    }

    // Checks for connected controllers
    private bool CheckInputDevices()
    {
        bool deviceConnected = false;
        //Get Joystick Names
        string[] temp = Input.GetJoystickNames();

        //Check whether array contains anything
        if (temp.Length > 0)
        {
            //Iterate over every element
            for (int i = 0; i < temp.Length; ++i)
            {
                //Check if the string is empty or not
                if (!string.IsNullOrEmpty(temp[i]))
                {
                    //Not empty, controller temp[i] is connected
                    Debug.Log("Controller " + i + " is connected using: " + temp[i]);
                    deviceConnected = true;
                }
                else
                {
                    //If it is empty, controller i is disconnected
                    //where i indicates the controller number
                    Debug.Log("Controller: " + i + " is disconnected.");
                    deviceConnected = false;
                }
            }
        }
        return deviceConnected;
    }
}
