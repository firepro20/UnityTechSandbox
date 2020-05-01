using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController instance;
    public static AudioController Instance { get { return instance; } }

    // Public Variables
    public AudioClip menuSelect;
    public AudioClip menuPress;
    public AudioClip dashClip;
    public AudioClip executionerDeath;
    public AudioClip droneDeath;
    public AudioClip soldierAttackClip;
    public AudioClip firstSwingClip;
    public AudioClip secondSwingClip;
    public AudioClip swordThrowClip;
    public AudioClip swordHitClip;
    public AudioClip playerHurt;
    public AudioClip[] runningClips;

    // Private Variables
    private AudioSource effectsAudioSource;
    private float startTime;
    private float firstClipStartTime;
    private float secondClipStartTime;

    void Awake()
    {
        /* Singleton */
        if (instance == null && instance != this)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }
        /* End of Singleton */
        
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioSource[] aSourceList = GetComponents<AudioSource>();
        foreach (AudioSource source in aSourceList)
        {
            if (source.clip == null)
            {
                effectsAudioSource = source;
            }
        }
    }

    public void PlayButtonSelect()
    {
        effectsAudioSource.PlayOneShot(menuSelect);
    }

    // Plays button press, delayed flag delays the clip playback by delay value
    public void PlayButtonPress(bool delayed = false, float delayValue = 4f)
    {
        if (!delayed)
        {
            effectsAudioSource.PlayOneShot(menuPress);
        }
        else
        {
            if (!effectsAudioSource.isPlaying)
            {
                effectsAudioSource.clip = menuPress;
                effectsAudioSource.Play();
                effectsAudioSource.PlayDelayed(delayValue);
            }
        }
    }

    public void PlayRunStep(float delayValue = 0.5f)
    {
        if (!effectsAudioSource.isPlaying)
        {
            int rand = Random.Range(0, runningClips.Length - 1);
            AudioClip temp = runningClips[rand];
            effectsAudioSource.pitch = Random.Range(0.1f, 1f);
            
            effectsAudioSource.clip = temp;
            effectsAudioSource.Play();
            effectsAudioSource.PlayDelayed(delayValue);
        }
        // Always set pitch back to max for next sound call
        effectsAudioSource.pitch = 1f;
    }

    // Move 

    public AudioClip PlayFirstSwing()
    {
        effectsAudioSource.PlayOneShot(firstSwingClip);
        startTime = Time.time;
        //firstClipStartTime = Time.time;
        return firstSwingClip;
    }

    public AudioClip PlaySecondSwing()
    {
        effectsAudioSource.PlayOneShot(secondSwingClip);
        startTime = Time.time;
        //secondClipStartTime = Time.time;
        return secondSwingClip;
    }

    public bool isPlaying(AudioClip clip)
    {
        if ((Time.time - startTime) >= clip.length)
        {
            return false;
        }
        return true;
    }

    public void PlaySwordThrow()
    {
        effectsAudioSource.PlayOneShot(swordThrowClip);
    }

    public void PlaySwordHit()
    {
        effectsAudioSource.PlayOneShot(swordHitClip);
    }

    public void PlayDash()
    {
        effectsAudioSource.PlayOneShot(dashClip);
    }

    public void PlayExecutionerDeath()
    {
        effectsAudioSource.PlayOneShot(executionerDeath);
    }

    public void PlayDroneDeath()
    {
        effectsAudioSource.PlayOneShot(droneDeath);
    }

    public void PlayerDamaged()
    {
        effectsAudioSource.PlayOneShot(playerHurt);
    }

    public void PlaySoldierAttack()
    {
        effectsAudioSource.PlayOneShot(soldierAttackClip);
    }

    // Clear effectsAudioSource
    public void ClearEffectsSource()
    {
        if (effectsAudioSource.clip != null)
        {
            effectsAudioSource.clip = null;
        }
    }
}
