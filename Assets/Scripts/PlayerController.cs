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
    public float playerSpeed = 5f;
    public float acceleration = 5f;
    public float maxPlayerSpeed = 20f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float dashDistance = 15f;
    public float dashCooldown = 4f;
    public Transform groundCheck; // empty GroundCheck object
    public float groundDistance = 0.4f; // radius of physics check sphere
    public LayerMask groundMask; // only check for ground

    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    private float currentPlayerSpeed;
    private Vector3 velocity;
    private float dashTimer;

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
        currentPlayerSpeed = playerSpeed;
        lastPosition = new Vector3(0f, 0f, 0f);
        dashTimer = dashCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    private void LateUpdate()
    {
        // Resets player back to last known grounded position
        if (transform.position.y < -40f)
        {
            transform.position = lastPosition;
        }
    }

    // Handles player movement
    private void PlayerMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // true if object hit is within groundmask layer
        
        // Keeps the player grounded and updates last know position for respawn
        if (isGrounded && velocity.y < -2f)
        {
            lastPosition = transform.position;
            velocity.y = -2f; // ensures player is fully grounded since we are using a sphere // keeps velocity from being too large
        }

        // Call Game Controller to check if controller has been detected and switch controls
        xAxis = Input.GetAxis("Horizontal"); // left right
        zAxis = Input.GetAxis("Vertical"); // forward vector

        //Increase or decrease the player movement speed
        if (zAxis > 0)
        {
            //Increase the player speed
            currentPlayerSpeed += acceleration * Time.deltaTime;
        }
               
        if (zAxis == 0)
        {
            currentPlayerSpeed -= acceleration * 2.0f * Time.deltaTime;
            // speed reduced by twice the amount of acceleration
        }
        
        //Clamp the speed
        currentPlayerSpeed = Mathf.Clamp(currentPlayerSpeed, 0f, maxPlayerSpeed);
        
        Vector3 movement = transform.right * xAxis + transform.forward * zAxis;
        characterController.Move(movement * currentPlayerSpeed * Time.deltaTime);
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // how much velocity we need to jump
        }

        // Dash Ability
        dashTimer -= Time.deltaTime;
        dashTimer = Mathf.Clamp(dashTimer, 0f, dashCooldown);
        
        if (Input.GetKeyDown(KeyCode.LeftShift)) // GetKeyDown checks for first time key was pressed, GetKey constantly checks and returns interrupt more than once
        {
            if (dashTimer <= 0f)
            {
                characterController.Move(transform.forward * dashDistance);
                dashTimer = dashCooldown; // Reset dashTimer when dash is called
            }
        }

        // Freefall formula, applies gravity over time
        velocity.y += gravity * Time.deltaTime;
        Debug.Log(velocity.y);
        characterController.Move(velocity * Time.deltaTime); 
        // End of Freefall
    }

    // Returns current player speed
    public float GetPlayerSpeed()
    {
        return currentPlayerSpeed;
    }
    
    
}
