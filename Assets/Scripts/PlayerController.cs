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
    
    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    
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
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    // Handles player movement
    private void PlayerMovement()
    {
        // Call Game Controller to check if controller has been detected and switch controls
        xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * xAxis + transform.forward * zAxis;
        characterController.Move(movement * playerSpeed * Time.deltaTime);
    }

    
    
    
}
