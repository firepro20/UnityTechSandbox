﻿using System.Collections;
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
    public float playerHealth = 10f;
    public float acceleration = 5f;
    public float maxPlayerSpeed = 20f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;
    public float dashDistance = 15f;
    public float dashCooldown = 4f;
    public Transform groundCheck; // empty GroundCheck object
    public float groundDistance = 0.4f; // radius of physics check sphere
    public LayerMask groundMask; // only check for ground

    // Sword Control
    public GameObject playerSword;

    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    private float currentPlayerSpeed;
    private Vector3 velocity;
    private float dashTimer;

    // Player Health
    private float pHealth;

    // Sword Control
    private bool isSwinging;

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
        isSwinging = false;
        if (!playerSword)
        {
            Debug.Log("Player Sword is not set in inspector!");
        }
        pHealth = playerHealth;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        SwordControl();
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
            // speed reduced by twice the amount of acceleration
            currentPlayerSpeed -= acceleration * 2.0f * Time.deltaTime;
            
            /* Slide Snippet */
            //characterController.Move(transform.TransformDirection(Vector3.forward) * Time.deltaTime);
            // multiply by slide duration on timer so that it stop after some time  
            // Newton's Second Law of Motion // Possible Inclusion https://www.youtube.com/watch?v=ckm7HhwNDvk
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
        
        characterController.Move(velocity * Time.deltaTime); 
        // End of Freefall
    }

    private void SwordControl()
    {
        // Left mouse button
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerSword != null)
        {
            if (!isSwinging)
            {
                Debug.Log("Clicked Left!");
                isSwinging = true;
                StartCoroutine(SwordAttack());
            }
            else
            {
                Debug.Log("Currently Swinging, please wait!");
            }
        }
       
        // Right mouse button
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            if (playerSword != null)
            {
                Debug.Log("Throw Weapon!");
                SwordController.Instance.ThrowSword();
            }
        }
    }
    
    private IEnumerator SwordAttack()
    {
        // SwordController.Instance.SwingSword();
        yield return new WaitForSeconds(2f);
        isSwinging = false;
        Debug.Log("Swing complete!");
    }

    public void TakeDamage(float damageAmount)
    {
        pHealth -= damageAmount;
    }

    // Returns current player speed
    public float GetPlayerSpeed()
    {
        return currentPlayerSpeed;
    }
    
    public float GetPlayerHealth()
    {
        return pHealth;
    }
    
}
