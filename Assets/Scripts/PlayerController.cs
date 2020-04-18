using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public float throwCooldown = 6f;
    public Transform groundCheck; // empty GroundCheck object
    public float groundDistance = 0.4f; // radius of physics check sphere
    public LayerMask groundMask; // only check for ground

    // Sword Control
    public GameObject playerSword;

    // Animator
    public Animator playerAnimator;

    // Speedometer
    public Image speedometer;

    // Clip Duration
    private float dashClip, deathClip, firstSlashClip, poseClip, secondSlashClip,
                  shieldClip, spinClip, swordPickupClip, swordThrowClip, swordlessIdleClip, swordlessWalkClip;
    // Clip List
    private AnimationClip[] clips;
    // Layer List
    //private AnimatorControllerLayer[] layers;

    // Private Variables //
    // Input Movement
    private float xAxis;
    private float zAxis;
    private float currentPlayerSpeed;
    private float speedRatio;
    private Vector3 velocity;
    private float dashTimer;
    private float throwTimer;

    // Player Health
    private float currentPlayerHealth;

    // Sword Control
    private bool isSwinging;
    private bool isFirstSlash;

    // Checkpoint 
    private Vector3 lastPosition; // last position of player

    // Movement Flags
    private bool toggleDash;
    private bool isGrounded;

    // Dash Light
    Light dashLight;
    Color colorRed = Color.red;
    Color colorGreen = Color.green;

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
        speedRatio = 0f;
        lastPosition = new Vector3(0f, 0f, 0f);
        dashTimer = dashCooldown;
        isSwinging = false;
        isFirstSlash = true;
        if (!playerSword)
        {
            Debug.Log("Player Sword is not set in inspector!");
        }
        currentPlayerHealth = playerHealth;
        UpdateAnimationClipTimes();

        //UpdateLayerWeight();

        // Dash light
        dashLight = GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.GameState())
        {
            PlayerStatus();
            PlayerMovement();
            SwordControl();
        }
    }

    private void LateUpdate()
    {
        // Resets player back to last known grounded position
        if (transform.position.y < -40f)
        {
            transform.position = lastPosition;
        }
    }

    // Gets animation clip speed and assigns them to floats
    private void UpdateAnimationClipTimes()
    {
        clips = playerAnimator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        { 
            switch (clip.name)
            {
                case "UI_Main_Dash":
                    dashClip = clip.length;
                    break;
                case "UI_Main_Death":
                    deathClip = clip.length;
                    break;
                case "UI_Main_First_Slash":
                    firstSlashClip = clip.length;
                    break;
                case "UI_Main_Pose_Walk":
                    poseClip = clip.length;
                    break;
                case "UI_Main_Second_Slash":
                    secondSlashClip = clip.length;
                    break;
                case "UI_Main_Shield_Break":
                    shieldClip = clip.length;
                    break;
                case "UI_Main_Spin":
                    spinClip = clip.length;
                    break;
                case "UI_Main_Sword_Pick_Up":
                    swordPickupClip = clip.length;
                    break;
                case "UI_Main_Sword_Throw":
                    swordThrowClip = clip.length;
                    break;
                case "UI_Main_Swordless_Idle":
                    swordlessIdleClip = clip.length;
                    break;
                case "UI_Main_Swordless_Walk":
                    swordlessWalkClip = clip.length;
                    break;
                default:
                    break;
            }
        }
    }

    // Retrieves Animator Weight
    /*
    private void UpdateLayerWeight()
    {
        layers = playerAnimatorController.layers;
        // remove first element which represents first layer, 
        //as default weight does not take into account first layer
        foreach (AnimatorControllerLayer animatorLayer in layers)
        {
            // case hold info on different weights
            Debug.Log(animatorLayer.name);
            Debug.Log(animatorLayer.defaultWeight);
        }
    }
    */

    // Checks player health
    private void PlayerStatus()
    {
        if (currentPlayerHealth <= 0 || Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("KeyPressed");
            if (!playerAnimator.GetBool("isDead"))
            {
                playerAnimator.SetBool("isDead", true);
                StartCoroutine(ClipDelay(deathClip));
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerAnimator.SetBool("isDead", false);
            SceneManager.LoadScene("MainScene");
        }
        if (Input.GetKeyDown(KeyCode.H)) // playerHit
        {
            playerAnimator.SetBool("isHit", true);
            StartCoroutine(ClipDelay(shieldClip, "isHit"));
        }
    }

    // Delays animation transition by clipDuration
    private IEnumerator ClipDelay(float clipDuration, string animationName = null, bool animState = false)
    {
        yield return new WaitForSeconds(clipDuration);
        StopCoroutine(ClipDelay(clipDuration, animationName));
        if (animationName != null)
        {
            playerAnimator.SetBool(animationName, animState);
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
            // Play footstep sounds
            AudioController.Instance.PlayRunStep(speedRatio);
        }
        else if (zAxis < 0 && currentPlayerSpeed != 0)
        {
            AudioController.Instance.PlayRunStep(speedRatio);
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

        // Increase spinner animation and speedometer fill
        speedRatio = currentPlayerSpeed / maxPlayerSpeed;
        speedRatio = Mathf.Clamp(speedRatio, 0f, 1f);
        playerAnimator.SetFloat("spinSpeed", speedRatio);
        speedometer.fillAmount = speedRatio;

        // Update dash cooldown light
        float colorTransition = dashTimer / dashCooldown;
        dashLight.color = Color.Lerp(colorGreen, colorRed, colorTransition);
        

        Vector3 movement = transform.right * xAxis + transform.forward * zAxis;
        characterController.Move(movement * currentPlayerSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // how much velocity we need to jump
        }

        /* Dash Ability */
        dashTimer -= Time.deltaTime;
        dashTimer = Mathf.Clamp(dashTimer, 0f, dashCooldown);

        // GetKeyDown checks for first time key was pressed, GetKey constantly checks and returns interrupt more than once
        if (Input.GetKeyDown(KeyCode.LeftShift)) 
        {
            if (dashTimer <= 0f)
            {
                playerAnimator.Play("UI_Main_Dash", 1, 0.3f); 
                characterController.Move(transform.forward * dashDistance);
                dashTimer = dashCooldown; // Reset dashTimer when dash is called

                // Play dash effect
                AudioController.Instance.PlayDash();
            }
        }
        else
        {
            
        }
        // Freefall formula, applies gravity over time
        velocity.y += gravity * Time.deltaTime;
        
        characterController.Move(velocity * Time.deltaTime); 
        // End of Freefall
    }

    private void SwordControl()
    {
        
        if (!isSwinging)
        {
            isFirstSlash = true;
        }
        else
        {
            isFirstSlash = false;
        }
        
        // Left mouse button
        if (Input.GetKeyDown(KeyCode.Mouse0) && SwordController.Instance.GetHoldingStatus())
        {
            if (!isSwinging && isFirstSlash) // first slash
            {
                if (playerAnimator.GetCurrentAnimatorStateInfo(1).length > playerAnimator.GetAnimatorTransitionInfo(1).normalizedTime)
                {
                    isSwinging = true;
                    playerAnimator.SetInteger("attackID", 0);
                    StartCoroutine(SwordAttack(firstSlashClip));

                    // Play first swing effect
                    if (!playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("UI_Main_First_Slash") && 
                        !playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("UI_Main_Second_Slash") &&
                        !AudioController.Instance.isPlaying(AudioController.Instance.PlayFirstSwing()))
                    {
                        Debug.Log("First - This should show but it is not being shown!");
                        AudioController.Instance.PlayFirstSwing();
                    }
                }
            }
            else if (isSwinging && !isFirstSlash)
            {
                if (playerAnimator.GetCurrentAnimatorStateInfo(1).length > playerAnimator.GetAnimatorTransitionInfo(1).normalizedTime) // && !inSwingState()
                {
                    playerAnimator.SetInteger("attackID", 1);
                    StartCoroutine(SwordAttack(secondSlashClip, true));
                    // Play second swing effect
                    if (!playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("UI_Main_First_Slash") &&
                       !playerAnimator.GetCurrentAnimatorStateInfo(1).IsName("UI_Main_Second_Slash") && 
                       !AudioController.Instance.isPlaying(AudioController.Instance.PlaySecondSwing()))
                    {
                        Debug.Log("Second - This should show but it is not being shown!");
                        AudioController.Instance.PlaySecondSwing();
                    }
                }
                // prevents attack click spam
            }
        }
        // Right Mouse Button
        /*
        if (Input.GetKeyDown(KeyCode.Mouse1) && SwordController.Instance.GetHoldingStatus())
        {
            if (!isSwinging) 
            {
                Debug.Log("Clicked Right!");
                isSwinging = true;
                playerAnimator.SetInteger("attackID", 1);
                StartCoroutine(SwordAttack(secondSlashClip));
                //Time.timeScale = 0.25f;
            }
        }
        */

        /* Throw Ability */
        throwTimer -= Time.deltaTime;
        throwTimer = Mathf.Clamp(throwTimer, 0f, throwCooldown);
        // Right mouse button
        if ((Input.GetKeyDown(KeyCode.Mouse2) || Input.GetKeyDown(KeyCode.F)) && throwTimer <= 0f)
        {
            if (playerSword != null)
            {
                Debug.Log("Throw Weapon!");
                SwordController.Instance.ThrowSword();
                throwTimer = throwCooldown;

                // Play sword throw
                AudioController.Instance.PlaySwordThrow();
            }
        }
    }

    private IEnumerator SwordAttack(float attackTime, bool secondSlash = false)
    {
        if (!secondSlash)
        {
            yield return new WaitForSeconds(attackTime);
            isSwinging = false;
            playerAnimator.SetInteger("attackID", -1);
            StopCoroutine(SwordAttack(attackTime));
        }
        else
        {
            yield return new WaitForSeconds(attackTime);
            isFirstSlash = true;
            playerAnimator.SetInteger("attackID", -1);
            StopCoroutine(SwordAttack(attackTime, secondSlash));
        }
        
    }

    public void TakeDamage(float damageAmount)
    {
        currentPlayerHealth -= damageAmount;
        if (currentPlayerHealth == 0)
        {
            currentPlayerHealth = 0;
        }
    }

    // Returns current player speed
    public float GetPlayerSpeed()
    {
        return currentPlayerSpeed;
    }
    
    public float GetPlayerHealth()
    {
        return currentPlayerHealth;
    }
    
}
