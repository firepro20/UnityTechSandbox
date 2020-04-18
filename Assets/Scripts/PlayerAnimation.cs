using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTestScript : MonoBehaviour
{
    private Animator Anim;

    // Start is called before the first frame update
    void Start()
    {
        Anim = gameObject.GetComponent<Animator>();

    }
    // Test Comment
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Anim.GetBool("IsWalking"))
            {
                Anim.SetBool("IsWalking", false);
            }
            else
            {
                Anim.SetBool("IsWalking", true);
            }
        }
        else if (Input.GetKey(KeyCode.C))
        {
            Anim.SetBool("Combat", true);
        }
        else if (Input.GetKey(KeyCode.R))
        {
            Anim.SetBool("Ranged", true);
        }
        else if (Input.GetKey(KeyCode.V))
        {
            Anim.SetBool("Victory", true);
        }
    }
}
