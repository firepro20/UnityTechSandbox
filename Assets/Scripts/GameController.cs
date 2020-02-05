using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    // Class References //


    // Public Variables //
    // Current Speed UI
    public TMPro.TextMeshProUGUI currentPlayerSpeed;

    // Private Variables //
    // Game Logic
    private int gameTime;

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
    }

    // Update is called once per frame
    void Update()
    {
        // Convert Time from float to int
        gameTime = (int)Time.time;
        
        // Check Input every 5 seconds since start of the game
        if (gameTime % 5 == 0)
        {
            CheckInputDevices();
        }

        // Updates Speed Text on UI
        currentPlayerSpeed.text = String.Format("Speed - {0:F0}", PlayerController.Instance.GetPlayerSpeed()); 
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
                    Debug.Log("Controller: " + i + " is disconnected.");
                    deviceConnected = false;
                }
            }
        }
        return deviceConnected;
    }

}
