using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    private static MenuController instance;
    public static MenuController Instance { get { return instance; } }

    // Public Variables
    public Canvas mainMenu;
    public Canvas settingsMenu;
    public AudioSource musicAudioSource;
    public AudioSource effectsAudioSource;

    // Private Variables
    private Slider backgroundSlider;
    private Slider effectSlider;
    private Toggle timerToggle;
    private AudioSource[] audioSourceList;

    private float volumeLevel;
    private float effectLevel;
    private bool timerStatus;

    private void Awake()
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
        
        // Sets respective audio source
        audioSourceList = AudioController.Instance.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource aSource in audioSourceList)
        {
            if (aSource.clip != null)
            {
                musicAudioSource = aSource;
            }
            else
            {
                effectsAudioSource = aSource;
            }
        }
        

        timerToggle = settingsMenu.GetComponentInChildren<Toggle>();
        Slider[] sliders = settingsMenu.GetComponentsInChildren<Slider>();

        foreach (Slider slider in sliders)
        {
            //if (slider.name.Contains("Volume"))
            switch (slider.name)
            {
                case "BackgroundSlider":
                    backgroundSlider = slider;
                    break;
                case "EffectSlider":
                    effectSlider = slider;                    
                    break;
            }
        }
        
        settingsMenu.gameObject.SetActive(false);

        // Set music and timer according to playerprefs set after setting save
        if (PlayerPrefs.HasKey("Volume") && PlayerPrefs.HasKey("Effects") && PlayerPrefs.HasKey("Timer"))
        { 
            backgroundSlider.value = PlayerPrefs.GetFloat("Volume");
            effectSlider.value = PlayerPrefs.GetFloat("Effects");
            int timerValue = PlayerPrefs.GetInt("Timer");
            if (timerValue == 1) { timerStatus = true; } else { timerStatus = false; }
            timerToggle.isOn = timerStatus;
        }
        else
        {
            // If no setting save found, default to designer master volume value
            backgroundSlider.value = musicAudioSource.volume;
            effectSlider.value = effectsAudioSource.volume;
        }

        // Restore mouse functionality
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Get all buttons and add listener to each
        Button[] buttonList = GameObject.Find("Menu").GetComponentsInChildren<Button>();
        foreach (Button button in buttonList)
        {
            // Click Sound
            button.onClick.AddListener(() => AudioController.Instance.PlayButtonPress());

            // Hover Sound
            EventTrigger hoverTrigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => AudioController.Instance.PlayButtonSelect());
            hoverTrigger.triggers.Add(entry);
        }

        // Slider Listener
        effectSlider.onValueChanged.AddListener(delegate { AudioController.Instance.PlayButtonPress(true); });

        // Toggle Listener
        timerToggle.onValueChanged.AddListener(delegate { AudioController.Instance.PlayButtonPress(true); });
    }

    // Update is called once per frame
    void Update()
    {
        // Adjust settings in real time
        // if ensures these have been set and avoids null exception on assignment
        if (musicAudioSource && effectsAudioSource)
        {
            volumeLevel = backgroundSlider.value;
            musicAudioSource.volume = volumeLevel;

            effectLevel = effectSlider.value;
            effectsAudioSource.volume = effectLevel;
        }
        
        timerStatus = timerToggle.isOn;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ToggleSettings()
    {
        if (mainMenu.isActiveAndEnabled)
        {
            mainMenu.gameObject.SetActive(!mainMenu.isActiveAndEnabled);
            settingsMenu.gameObject.SetActive(true);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void SaveSettings()
    {
        // implement player prefs here
        PlayerPrefs.SetFloat("Volume", volumeLevel);
        PlayerPrefs.SetFloat("Effects", effectLevel);

        int timerValue = -1;
        if (timerStatus) { timerValue = 1; } else { timerValue = 0; }
        PlayerPrefs.SetInt("Timer", timerValue);

        // when ready, disable settings menu and go back to main menu
        if (settingsMenu.isActiveAndEnabled)
        {
            settingsMenu.gameObject.SetActive(false);
            mainMenu.gameObject.SetActive(true);
        }

        if (effectsAudioSource.clip != null)
        {
            effectsAudioSource.clip = null;
        }
        PlayerPrefs.Save();
    }

    // On Main Menu loaded functionality
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Sets effects audio source for audio controller instance on load
        // Avoids bug when audioSourceList is empty on load
        audioSourceList = AudioController.Instance.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource aSource in audioSourceList)
        {
            if (aSource.clip != null)
            {
                musicAudioSource = aSource;
            }
            else
            {
                effectsAudioSource = aSource;
            }
        }
    }

    public float GetMasterVolume()
    {
        return volumeLevel;
    }

    public float GetEffectVolume()
    {
        return effectLevel;
    }

    public bool isTimerEnabled()
    {
        return timerStatus;
    }
    }
