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
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public Transform groundCheck; // empty GroundCheck object
    public float groundDistance = 0.4f; // radius of physics check sphere
    public LayerMask groundMask; // only check for ground

    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    private Vector3 velocity;

    // Checkpoint 
    private Vector3 lastPosition; // last position of player

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
        // Initialize all variables
        lastPosition = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    // Handles player movement
    private void PlayerMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // true if object hit is within groundmask layer
        Debug.Log("Transform y is - " + transform.position.y);
        if (transform.position.y < -5f)
        {
            transform.position = lastPosition;
            Debug.Log("Triggered!");
        }

        if (isGrounded && velocity.y < 0)
        {
            lastPosition = transform.position;
            Debug.Log("lastPosition - " + lastPosition);
            velocity.y = -2f; // ensures player is fully grounded since we are using a sphere
        }

        // Call Game Controller to check if controller has been detected and switch controls
        xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * xAxis + transform.forward * zAxis;
        characterController.Move(movement * playerSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // how much velocity we need to jump
        }

        // Freefall formula
        velocity.y += gravity * Time.deltaTime;
        
        characterController.Move(velocity * Time.deltaTime); 
    }

    
    
    
}
