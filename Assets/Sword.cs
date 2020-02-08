using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    // Public Variables

    // Private Variables
    private Quaternion startAngle;
    private Animator swordAnimator;

    // Start is called before the first frame update
    void Start()
    {
        startAngle = transform.rotation;
        swordAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Started!"); 
            SwingSword(0.5f);
        }
    }

    private void SwingSword(float delay)
    {
        StartCoroutine(Swing(delay));
    }

    private IEnumerator Swing(float delay)
    {
        // play animation from animator
        swordAnimator.SetBool("Swing", true); 
        yield return new WaitForSeconds(delay);
        swordAnimator.SetBool("Swing", false);
        StopCoroutine(Swing(delay));
    }
}
