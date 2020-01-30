using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance { get { return instance; } }

    // Class References //


    // Public Variables //

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
        
        // Check Input every 4 seconds since start of the game
        if (gameTime % 5 == 0)
        {
            CheckInputDevices();
        }
    }

    // Checks for connected controllers
    public bool CheckInputDevices()
    {
        bool deviceConnected = false;
        //Get Joystick Names
        string[] temp = Input.GetJoystickNames();
        Debug.LogWarning("Called! At " + gameTime);
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
