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
    

    // Internal Variables
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform swordParent;
    private Transform rootParent;
    private SphereCollider swordCollider;
    
    private bool isHolding = true;
    private bool swordPickupDelay = false;
    private float pickupDelay = 0.7f;


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
        rootParent = transform.root;
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
            sword.transform.SetParent(swordParent); // right wrist
            Time.timeScale = 1f;
        }
        else
        {
            sword.transform.SetParent(null);
            Time.timeScale = 0.4f;
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
        if (other.gameObject.CompareTag("Player") && !isHolding && swordPickupDelay)
        {
            Debug.Log("Sword Picked Up!");
            isHolding = true;
            swordPickupDelay = false;
            PlayerController.Instance.playerAnimator.SetInteger("attackID", -1); // change to pickup, change to idle?
        }
    }

    public void ThrowSword()
    {
        if (isHolding) 
        {
            StartCoroutine(SwordPickupDelay(pickupDelay));
            isHolding = false;
            Transform tempParent = swordParent;
            swordParent = rootParent; // shoot from Player transform, which is the root parent
            sword.transform.position = shootPoint.position;
            sword.transform.rotation = new Quaternion(Camera.main.transform.rotation.x, swordParent.transform.rotation.y, swordParent.transform.rotation.z, swordParent.transform.rotation.w);
            //Vector3 localForward = transform.parent.InverseTransformDirection(transform.forward);
            sword.GetComponent<Rigidbody>().useGravity = true;
            sword.GetComponent<Rigidbody>().detectCollisions = true;
            float forceMultiplier = 2f; // 0f
            
            sword.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwForce * forceMultiplier); // swordParent.transform.forward // fires from pivot3
                                                                                                                  
            // Call animator to play sword throw clip
            PlayerController.Instance.playerAnimator.Play("UI_Main_Sword_Throw", 1, 0.5f);

            // reset parent
            swordParent = tempParent;
        }
    }

    public void SwingSword()
    {

    }

    private IEnumerator SwordPickupDelay(float pickupDelay)
    {
        yield return new WaitForSeconds(pickupDelay);
        swordPickupDelay = true;
        StopCoroutine(SwordPickupDelay(pickupDelay));
    }

    public bool GetHoldingStatus()
    {
        return isHolding;
    }
}
