using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBlink : MonoBehaviour
{
    public GameObject staticSprite;
    public GameObject actionSprite;
    public float blinkTime = 1f;

    void Start()
    {
        staticSprite.GetComponent<SpriteRenderer>().enabled = true;
        actionSprite.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(StartBlinking()); 
    }

    IEnumerator StartBlinking()
    {
        yield return new WaitForSeconds(blinkTime); 
        staticSprite.GetComponent<SpriteRenderer>().enabled = !staticSprite.GetComponent<SpriteRenderer>().enabled; //This toggles it
        actionSprite.GetComponent<SpriteRenderer>().enabled = !actionSprite.GetComponent<SpriteRenderer>().enabled;
        StartCoroutine(StartBlinking());
    }
}
