using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    // Class References //


    // Public Variables //
    public Canvas pauseMenu;
    public Canvas hud;
    public TextMeshProUGUI gameTimeText;
    public float timeMultiplier = 1f;

    // Private Variables //
    // Game Logic
    private int gameTime;
    private float gameTimer;
    private bool timerStatus;
    private bool isPlaying;
    private bool gameOver;

    // Prop and Enemy Management
    private Prop[] propList;

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
        gameTime = 0;
        gameTimer = 0;
        gameTimeText.text = "0";

        // Timer Toggle
        if (PlayerPrefs.HasKey("Timer"))
        {
            int timerValue = PlayerPrefs.GetInt("Timer");
            if (timerValue == 1) { timerStatus = true; } else { timerStatus = false; }
        }
        else
        {
            // By default timer is not shown
            timerStatus = false;
        }
        hud.gameObject.SetActive(timerStatus);

        /* Duplicate code detected! Ideally make a utility class? */
        // Get all buttons and add listener to each
        Button[] buttonList = GameObject.Find("PauseMenu").GetComponentsInChildren<Button>();
        if (AudioController.Instance)
        {
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
        }

        // Get all external audio sources set them to effects volume
        // Get objects of type Prop
        propList = UnityEngine.Object.FindObjectsOfType<Prop>();
        foreach(Prop prop in propList)
        {
            prop.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("Effects");
        }

        // Game Started
        isPlaying = true;
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        GameFlow();
        UpdateTimeText();

        // Convert Time from float to int
        gameTime = (int)Time.time;

        // Check Input every 5 seconds since start of the game
        if (gameTime % 5 == 0)
        {
            CheckInputDevices();
        }
    }

    private void GameFlow()
    {
        if (isPlaying)
        {
            Time.timeScale = 1;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (isPlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            isPlaying = false;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !isPlaying)
        {
            Resume();
        }

        if (NoEnemiesRemaining())
        {
            Time.timeScale = 0;
            // show timer when game ends
            timerStatus = true;
            isPlaying = false;
            gameOver = true; 
            hud.gameObject.SetActive(timerStatus);
        }
      
        if (gameOver)
        {
            foreach(RectTransform menuOption in pauseMenu.GetComponentsInChildren<RectTransform>())
            {
                if (menuOption.name == "Resume")
                {
                    menuOption.gameObject.transform.parent.GetComponentInChildren<TextMeshProUGUI>().text = "Complete!";
                    menuOption.gameObject.SetActive(false);
                }
            }
            pauseMenu.gameObject.SetActive(!isPlaying);
        }
        else
        {
            pauseMenu.gameObject.SetActive(!isPlaying);
        }
    }

    private void UpdateTimeText()
    {
        float min, sec;
        gameTimer = Time.timeSinceLevelLoad;

        sec = Mathf.Floor(gameTimer) % 60 * timeMultiplier;
        min = Mathf.Floor((gameTimer % 3600) / 60);
        //hrs = Mathf.Floor(gameTimer / 3600);
        //gameTimeText.text = System.String.Format("{0:00}:{1:00}:{2:00}", hrs, min, sec);
        gameTimeText.text = System.String.Format("{0:00}:{1:00}", min, sec);
    }

    // Checks for connected controllers
    public bool CheckInputDevices()
    {
        bool deviceConnected = false;
        //Get Joystick Names
        string[] temp = Input.GetJoystickNames();
        
        //Check whether array contains anything
        if (temp.Length > 0)
        {
            //Iterate over every element
            for (int i = 0; i < temp.Length; ++i)
            {
                //Check if the string is empty or not
                if (!string.IsNullOrEmpty(temp[i]))
                {
                    //Not empty, controller temp[i] is connected
                    Debug.Log("Controller " + i + " is connected using: " + temp[i]);
                    deviceConnected = true;
                }
                else
                {
                    //If it is empty, controller i is disconnected
                    //where i indicates the controller number
                    deviceConnected = false;
                }
            }
        }
        return deviceConnected;
    }

    // On Main scene loaded functionality

    /*
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("OnEnable");
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Debug.Log("OnDisable");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }
    */

    private bool NoEnemiesRemaining()
    {
        return GameObject.FindObjectsOfType<BigBoiAI>().Length <= 0 && GameObject.FindObjectsOfType<DroneAI>().Length <= 0;
    }

    public void Resume()
    {
        isPlaying = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void Exit()
    {
        // Clear effectAudioSource
        AudioController.Instance.ClearEffectsSource();

        SceneManager.LoadScene("MainMenu");
    }
    
    // Return isPlaying state
    public bool GameState()
    {
        return isPlaying;
    }
}
