using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigBoiAI : MonoBehaviour
{
    // Health
    public float health = 50f;
    private float currentEnemyHealth;
    // Damage
    public float attackDamage = 10f;
    // Pathfinding //
    NavMeshAgent theAgent;
    // Patrolling //
    public Transform pathHolder; // Holds the path in which the enemy follows
    public float patrolSpeed = 5; // The enemy's base patrolSpeed
    public float waypointWaitTime = .3f;
    public float turnpatrolSpeed = 90;
    private float originalSpeed = 0;
    //Enemy Field of View //
    public Light spotlight;
    public float viewDistance;
    public float attackDistance;
    public LayerMask viewMask;
    private float viewAngle;
    //Enemy Field of View for player //
    Transform player;
    Color originalSpotLightColor;
    //Big Boi Finite State Machine //
    bool HasSeenPlayer = false;
    private enum State { Idle = 0, Alert = 1, Patrol = 2, Chase = 3, Attack = 4, Dead = 5 };
    private State aiState = State.Idle;
    private Vector3 currentPosition;
    private Vector3 currentAngle;
    private bool CoroutineHasStarted = false;
    private bool PlaceWaypointsOnce = false;
    private bool onlyCheckUnseenOnce = true;
    //Timer Code //
    public float idleTime = 30f; //Idle timer//
    private float idleDelay; //Idle timer//
    public float wanderTime = 3f; //Alert timer//
    private float wanderDelay; //Alert timer//
    private float attackDelay = 2f;
    private bool canDamage = true;
    //Animations//
    public Animator bigBoiAnimator;

    // Object Reference
    private DroneAI[] drones;

    // Start is called before the first frame update
    void Start()
    {
        // Remember what speed we had origionally
        originalSpeed = patrolSpeed;

        // Call the Player tag into this code
        player = GameObject.FindGameObjectWithTag("Player").transform;
    
        // Get all objects of type droneAI
        drones = FindObjectsOfType<DroneAI>();

        // Set the enemy view angle
        viewAngle = spotlight.spotAngle;
        // Keep spotlight color for enemy sight
        originalSpotLightColor = spotlight.color;

        // Pathfinding //
        theAgent = GetComponent<NavMeshAgent>();

        // Assign Idle time to Delay for AI
        idleDelay = idleTime;
        // Assign Alert time to delay new nav point for AI
        wanderDelay = wanderTime;

        // Animator
        bigBoiAnimator = GetComponentInChildren<Animator>();

        currentEnemyHealth = health;
    }

    // Called at set interval each time. Good for physics
    void FixedUpdate()
    {
        //Debug.Log("We are in" + aiState);

        switch (aiState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Alert:
                Alert();
                break;
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
            case State.Dead:
                Dead();
                break;
            default:
                break;
        }

        if (CanSeePlayer() == true)
        {
            // We have seen the player, reset the once off check if we can see him.
            onlyCheckUnseenOnce = false;

            // If the enemy can see the player, we want to begin a chase.
            aiState = State.Chase;
        }
        else
        {
            // So we know that we can't see the player, we only need to know this once.
            if (onlyCheckUnseenOnce == false)
            {
                //the player is not inside the view angle, check to see if we have seen the player before or not
                //The decide a state based on that.
                if (!HasSeenPlayer)
                {
                    aiState = State.Patrol;
                }
                else
                {
                    if (aiState != State.Attack)
                    {
                        aiState = State.Alert;
                    }
                }
                onlyCheckUnseenOnce = true;
            }
        }

        if (droneInRange() == true)
        {
            foreach (DroneAI drone in drones)
            {
                if (drone.aiState == DroneAI.State.Alert && HasSeenPlayer == false)
                {
                    aiState = State.Alert;
                }

                if (drone.aiState == DroneAI.State.Chase)
                {
                    aiState = State.Chase;
                }
            }

            //Debug.Log("The drone is now following the BigBoi");
            patrolSpeed = originalSpeed * 2;
            theAgent.speed = originalSpeed * 2;
        }
        else
        {
            patrolSpeed = originalSpeed;
            theAgent.speed = originalSpeed;
        }
    }

    bool droneInRange()
    {
        // If the enemy is dead, we don't want him to be able to see the player anymore (protection from zombie bugs)
        if (aiState != State.Dead)
        {
            foreach (DroneAI drone in drones)
            {
                // First we need to see if the drone is within range
                // To optimise we could chack square distances instead
                if (Vector3.Distance(transform.position, drone.transform.position) < viewDistance)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Trigger to move to chase state
    bool CanSeePlayer()
    {
        // If the enemy is dead, we don't want him to be able to see the player anymore (protection from zombie bugs)
        if (aiState != State.Dead)
        {
            // First we need to see if the player is within range
            // To optimise we could chack square distances instead
            if (Vector3.Distance(transform.position, player.position) < viewDistance)
            {
                // Check where the player is in relation to the view angle
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
                // If the player is inside the view angle then do something
                if (angleBetweenGuardAndPlayer < viewAngle / 2f)
                {
                    // Is the line of sight blocked by an obstacle? Check via linecasting (raycasting but limited)
                    if (!Physics.Linecast(transform.position, player.position, viewMask))
                    {
                        // Right, so it is within the cone of vision, we are being chased and nothing is blocking our line of sight, is he close enough to attack?
                        if (Vector3.Distance(transform.position, player.position) < attackDistance)
                        {
                            //We only want to hit the player once, and then move to idle. 
                            //So if we get inside this if statement then we will switch to idle at the end of attack
                            //Notrunning this immediently again.
                            if (aiState != State.Attack && aiState != State.Idle)
                            {
                                //https://i1.sndcdn.com/avatars-000490738590-koal8q-t500x500.jpg
                                bigBoiAnimator.SetBool("isAttacking", true);
                                aiState = State.Attack;
                            }
                        }
                        else
                        {
                            bigBoiAnimator.SetBool("isAttacking", false);
                            // Nothing is blocking line of sight, but we are not in attack range so chase!
                            return true;
                        }
                    }
                }
                else
                {
                    // If the player is in range but he ran behind us, we lost sight of him. Check what to do when we cant see him in update.
                    bigBoiAnimator.SetBool("isAttacking", false);
                    onlyCheckUnseenOnce = false;
                    return false;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
            // Start moveing through each waypoint in the array
            transform.position = waypoints[0];

            int targetWaypointIndex = 1;
            // Vector 3 for the position of the target waypoint
            Vector3 targetWaypoint = waypoints[targetWaypointIndex];
            transform.LookAt(targetWaypoint);

            // Have this for as long as the enemy is in a patrol state
            while (aiState == State.Patrol)
            {

                // We want to get the enemy instance to move towards the current target waypoint at a set patrolSpeed
                transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, patrolSpeed * Time.deltaTime);

                // if the enemy has reached the current target waypoint then...
                float distanceCheck = Vector3.Distance(transform.position, targetWaypoint);

                if (distanceCheck <= 0.5f)
                {
                    //...we want to move onto the next waypoint
                    // When the waypoint array has reached its limit, we should return it to zero.
                    targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                    targetWaypoint = waypoints[targetWaypointIndex];
                // When we reach the way point we want to pause for the wait time//

                    // The enemy is now idle, we want them to wait and then move
                    yield return new WaitForSeconds(waypointWaitTime);
                    //aiState = State.Idle;

                    // Using pythragoras, we want to calculate how much the enemy has to turn to look towards a new objective
                    Vector3 dirToLookTarget = (targetWaypoint - transform.position).normalized;
                    float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

                    // We only want it to loop while the angle in which the enemy is looking is != to the angle we want the enemy to look in
                    // using 0.05 because it is safer than 0, (may never reach perfect 0) 
                    while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
                    {
                        // Actually get the enemy to turn
                        float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnpatrolSpeed * Time.deltaTime);
                        transform.eulerAngles = Vector3.up * angle;
                        yield return null;
                    }
                }
                yield return null;
            }
    }

    // Visually represent the waypoints and the paths in the editor
    void OnDrawGizmos()
    {
        //set up the path that the AI will follow
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;
        // Drawing the waypoints in the editor
        foreach (Transform waypoint in pathHolder)
        {
            //creates sphere for each one
            Gizmos.DrawSphere(waypoint.position, 0.3f);
            //show the path that the AI will follow
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        // Connect the waypoint so that they go full circle
        Gizmos.DrawLine(previousPosition, startPosition);

        // Show the view angle of the enemies in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
        Gizmos.DrawRay(transform.position, transform.forward * attackDistance);
    }

    void GetRandomAngle()
    {
        // get the current y angle, and over time, call a method to offset this angle by -x +x from current angle every z seconds
        currentAngle = this.transform.eulerAngles;
    }
    //////////////////////////////////
    /// ENEMY FINITE STATE MACHINE ///
    //////////////////////////////////

    private void Idle()
    {
        //Start the idle animation
        bigBoiAnimator.SetBool("isIdle", true);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", false);

        //Set up and run the timer
        idleDelay -= Time.deltaTime;
        idleDelay = Mathf.Clamp(idleDelay, 0f, idleTime);
        //Debug.Log("BigBoi currently in Idle, leaving state in " + idleDelay);

        // When the timer has finished, check which state to move into
        if (idleDelay <= 0f)
        {
            //Debug.Log("We are inside idleDelay");

            if (HasSeenPlayer == false)
            {
                CoroutineHasStarted = false;
                aiState = State.Patrol;
            }
            else
            {
                aiState = State.Alert;
            }
            idleDelay = idleTime;
        }
    }

    private void Patrol()
    {
        //Start the patrol animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", true);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", false);


        // We don't want this to run infinitely, bool "switch gate" to turn it off.
        if (CoroutineHasStarted == false)
        {
                // We want to get the enemy to understand what these way point mean, use for loop to collect them all
                // and put them into an array
                Vector3[] waypoints = new Vector3[pathHolder.childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = pathHolder.GetChild(i).position;

                    // So that the enemy doesn't clip through the floor, change the waypoints y axis
                    waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
                }
            
            StartCoroutine(FollowPath(waypoints));
            // Close the bool "gate"
            CoroutineHasStarted = true;
        }
    }

    private void Chase()
    {
        //Play chase animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", true);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", false);

        // the enemies will go toward the player
        theAgent.SetDestination(player.transform.position);
        spotlight.color = Color.red;
        // We will never be fully idle again, we can only ever be alert now.
        HasSeenPlayer = true;
    }
 
    private void Alert()
    {
        //Start the alert animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", true);
        bigBoiAnimator.SetBool("isDead", false);

        // change light color to yellow
        spotlight.color = Color.yellow;

        wanderDelay -= Time.deltaTime;
        wanderDelay = Mathf.Clamp(wanderDelay, 0f, wanderTime);
        //Debug.Log("Creating new alert waypoint in " + wanderDelay);

        // When the timer has finished, check which state to move into
        if (wanderDelay <= 0f)
        {
            currentPosition = this.transform.position;
            float wanderRange = 8f;
            Vector3 randomDestination = currentPosition + new Vector3(UnityEngine.Random.Range(-wanderRange, wanderRange), 0, UnityEngine.Random.Range(-wanderRange, wanderRange));
            theAgent.SetDestination(randomDestination);
            wanderDelay = wanderTime;
        }
    }

    private void Attack()
    {
        //Start the attack animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", false);
        bigBoiAnimator.SetBool("isAttacking", true);

        //Debug.Log("We are now attacking");
        //aiState = State.Idle;
    }

    private void Dead()
    {
        //Start the idle animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", false);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", true);

        //Update the drone list to make sure it knows he is dead
            foreach (DroneAI drone in drones)
            {
                if (drone.nearestAlly = this.gameObject)
                {
                   drone.RefreshAllyList();
                }
            }
        
    }

    public void TakeDamage(float damageAmount)
    {
        //Start the hit animation
        bigBoiAnimator.SetBool("isIdle", false);
        bigBoiAnimator.SetBool("isPatrolling", false);
        bigBoiAnimator.SetBool("seesPlayer", false);
        bigBoiAnimator.SetBool("isHit", true);
        bigBoiAnimator.SetBool("isAttacking", false);
        bigBoiAnimator.SetBool("isLookingAround", false);
        bigBoiAnimator.SetBool("isDead", false);

        currentEnemyHealth -= damageAmount;
        if (currentEnemyHealth == 0)
        {
            currentEnemyHealth = 0;
        }

        if (currentEnemyHealth == 0)
        {
            aiState = State.Dead;
        }
    }

    //////////////////////////
    /// ANIMATOR FUNCTIONS ///
    //////////////////////////

        // ATTACK
    public void attackAnimationBegin()
    {
        theAgent.speed = 0;
        AudioController.Instance.PlaySoldierAttack();
    }

    public void checkToDamage()
    {
        if (aiState == State.Attack && canDamage)
        {
            PlayerController.Instance.TakeDamage(attackDamage);
            canDamage = false;
        }
    }

    public void attackAnimationEnd()
    {
        theAgent.speed = originalSpeed;
        canDamage = true;
    }

    // HIT
    public void hitOver()
    {
        aiState = State.Alert;
    }

    // DEATH
    public void deathAnimationBegin()
    {
        // We don't want him moving as he dies, the animator will handle that.
        theAgent.speed = 0;
        patrolSpeed = 0;
        AudioController.Instance.PlayExecutionerDeath();
        spotlight.color = Color.clear;
    }

    public void deathAnimationEnd()
    {
        //When he is finished dying, delete him from the current run
        this.enabled = false;
        gameObject.SetActive(false);
    }
}