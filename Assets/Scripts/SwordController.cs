using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private static SwordController instance;
    public static SwordController Instance { get { return instance; } }

    // Public Variables
    public float throwForce = 800f;
    public GameObject sword;
    public Transform shootPoint;
    public bool isHolding = true;

    // Internal Variables
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private SphereCollider swordCollider;
    private Transform swordParent;

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
        originalPosition = transform.localPosition; 
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;
        Debug.Log("Information - " + originalPosition + "\t" + originalRotation + "\t" + originalScale);
        swordCollider = GetComponentInChildren<SphereCollider>();
        if (!swordCollider)
        {
            Debug.Log("Sword Collider not retrieved!");
        }
        swordParent = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        // check if isHolding
        if (isHolding)
        {
            sword.transform.localPosition = originalPosition;
            sword.transform.localRotation = originalRotation;
            sword.transform.localScale = originalScale;
            sword.GetComponent<Rigidbody>().velocity = Vector3.zero;
            sword.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            sword.GetComponent<Rigidbody>().useGravity = false;
            sword.transform.SetParent(swordParent); // player
        }
        else
        {
            sword.transform.SetParent(null);
            // apply force
        }
    }

    private void LateUpdate()
    {
        if (transform.position.y < -20f)
        {
            transform.position = Vector3.one;
        }
    }

    // Hits player, set sword to original holding position
    private void OnCollisionEnter(Collision collision)
    {
        // Damage enemies

        /*
        // go through all contact points and find out which child collider was involved in collisio
        Collider myCollider = collision.contacts[0].thisCollider;
        if (myCollider.gameObject.CompareTag("Player"))
        {
            Debug.Log("Sword Picked up!");
            
        }
        */
    }

    // called once when object enters trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isHolding)
        {
            Debug.Log("Sword Picked Up!");
            isHolding = true;
        }
    }

    public void ThrowSword()
    {
        if (isHolding) // might cause issues as isHolding rules in update might interfere
        {
            isHolding = false;
            sword.transform.position = shootPoint.position;
            sword.transform.rotation = new Quaternion(Camera.main.transform.rotation.x, swordParent.transform.rotation.y, swordParent.transform.rotation.z, swordParent.transform.rotation.w);
            //Vector3 localForward = transform.parent.InverseTransformDirection(transform.forward);
            sword.GetComponent<Rigidbody>().useGravity = true;
            sword.GetComponent<Rigidbody>().detectCollisions = true;
            float forceMultiplier = 0f;
            if (PlayerController.Instance.GetPlayerSpeed() == forceMultiplier)
            {
                forceMultiplier = 0f;
            }
            else if (PlayerController.Instance.GetPlayerSpeed() > 0f && PlayerController.Instance.GetPlayerSpeed() <= 5f)
            {
                forceMultiplier = 1f;
            }
            else
            {
                forceMultiplier = 2f;
            }
            sword.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwForce * forceMultiplier); // swordParent.transform.forward // fires from pivot3
            //Debug.LogError("Launched!");
        }
    }

    public void SwingSword()
    {

    }
}
