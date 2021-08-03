
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JasHandExperiment.Configuration;
using JasHandExperiment;
using System;
using CommonTools;
using System.IO;
using System.Globalization;

public class KeyBoardConroller : MonoBehaviour {
    private float BUTTON_UP_COOLDOWN = 0.05f;

    //Animator keyboard;
    BaseHandMovementFileDevice mDevice;
    CSVFile mWriteFile; 
    HandType handToAnimate; //side of hand show on screen
    HandType activeHand = HandType.Left;// side of hand the participent uses (on active mode only), set left as default
    int pressedInput;
    string lastDTUpdated;
    private Transform bb, gb, rb, yb;
    private Vector3 bbUp, gbUp, rbUp, ybUp, bbDown, gbDown, rbDown, ybDown;
    private float cooldown = 0.0f;
    private DateTime logStartTime;
    private bool doLogging = false;
    private KeyPressedData key = null;

    // Use this for initialization
    void Start () {
        if (CalibrationManager.Mode != HandPlayMode.RealTime)
        {
            return;
        }
        // get keyboard component from unity
        //keyboard = GetComponent<Animator>();
        //ask configuration which hand will show up on screnn 
        handToAnimate = ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate;

        var exType = ConfigurationManager.Instance.Configuration.ExperimentType;
        if (exType == ExperimentType.PassiveSimulation)
        {
            //read from input file as passive simulatiom
            mDevice = HandMovemventDeviceFactory.GetOrCreate(exType) as SimulationFileDevice;
            mDevice.Open();
        }
        else if (exType == ExperimentType.PassiveWatchingReplay)
        {
            //read from input file as passive watching replay
            mDevice = HandMovemventDeviceFactory.GetOrCreate<KeyBoardSimulationFileDevice>();
            mDevice.Open();
        }
        else if (exType == ExperimentType.Active)
        {
            //ask configuration which hand is activly typing right now
            activeHand = HandType.Left; //on active we always recored as if the hand is the left one
                                        //we change the hand on screen in other configuration
            mWriteFile = new CSVFile(); //some details indicates where to save the keybaord inputs
            var path = CommonUtilities.GetParticipantCSVFileName(ConfigurationManager.Instance.Configuration.OutputFilesConfiguration.UserPressesLogPath);
            // passice cause that's the columns of input n passive
            var columns = CommonUtilities.CreateGlovesDataFileColumns(ExperimentType.PassiveSimulation);
            mWriteFile.Init(new FileStream(System.Environment.CurrentDirectory + path, FileMode.Create), ',', columns);
        }

        bb = GameObject.Find("blueButton").GetComponent<Transform>();
        gb = GameObject.Find("greenButton").GetComponent<Transform>();
        rb = GameObject.Find("redButton").GetComponent<Transform>();
        yb = GameObject.Find("YellowButton").GetComponent<Transform>();

        bbUp = bb.localPosition;
        gbUp = gb.localPosition;
        rbUp = rb.localPosition;
        ybUp = yb.localPosition;
        Vector3 btnPressTranslation = new Vector3(0.0f, -0.37f, 0.0f);
        bbDown = bbUp + btnPressTranslation;
        gbDown = gbUp + btnPressTranslation;
        rbDown = rbUp + btnPressTranslation;
        ybDown = ybUp + btnPressTranslation;
    }

    public void OnDestroy()
    {
        if (mWriteFile != null)
        {
            mWriteFile.Close();
        }
        if (mDevice != null)
        {
            mDevice.Close();
        }
    }

    // Update is called once per frame
    void Update ()
    {        
        cooldown = Mathf.Max(cooldown - Time.deltaTime, 0.0f);
        if (CalibrationManager.Mode != HandPlayMode.RealTime)
        {
            return;
        }

        if (ConfigurationManager.Instance.Configuration.ExperimentType != ExperimentType.Active)
        // getting info from file
        {
            //check if there is some key to presse
            key = mDevice.GetHandData() as KeyPressedData;
            if (key == null || string.IsNullOrEmpty(key.KeyPressed)|| key.TimeStamp.Equals(lastDTUpdated))
            {
                SetAllButtonsOf(); // no data to update according to file
            }
            else
            {
                lastDTUpdated = key.TimeStamp;
                pressedInput = int.Parse(key.KeyPressed);
                if (handToAnimate.Equals(HandType.Right))
                {
                    ///////////ADVA- IT SEEMS LIKE WE DONT NEED THAT IF STATMANT OR THERE IS A MISSSING LINE:

                   // pressedInput = 4 - pressedInput + 1;   //we swiched the numbers on the phisical keyBoard
                    SetPressedButton();
                }
                else if (handToAnimate.Equals(HandType.Left)) // just to avoid the 'none' hand type case
                {
                    SetPressedButton();
                }
            }
        }
        else if (ConfigurationManager.Instance.Configuration.ExperimentType == ExperimentType.Active)
        // getting info directly from keyboard
        {
            SetUpButtonsOf();
            SetPressedButton();
        }
    }

    public KeyPressedData getKeyPressedData()
    {
        return key;
    }

    public void startLogging()
    {
        if (ConfigurationManager.Instance.Configuration.ExperimentType == ExperimentType.Active)
        {
            doLogging = true;
            logStartTime = DateTime.Now;
        }
    }

    public void pauseLogging()
    {
        if (ConfigurationManager.Instance.Configuration.ExperimentType == ExperimentType.Active)
            doLogging = false;
    }

    public void resumeLogging()
    {
        if (ConfigurationManager.Instance.Configuration.ExperimentType == ExperimentType.Active)
            doLogging = true;
    }

    private void SetPressedButton()
    {
        string pressedFinger = string.Empty;
        //SetUpButtonsOf();
        //the diffrence between handeling right hand and left hand is only 
        //in the data that been writing to file, so it is relavent only to active mode
        if (activeHand.Equals(HandType.Left))
        {
            pressedFinger = setUsingLeftHand(pressedFinger);
        }
        else if (activeHand.Equals(HandType.Right))
        {
            pressedFinger = setUsingRightHand(pressedFinger);
        }
        // writing to file
        if (doLogging && !string.IsNullOrEmpty(pressedFinger))
        {
            string line = string.Format("{0},{1}", DateTime.Now - logStartTime, pressedFinger);
            mWriteFile.WriteLine(line);
        }
    }

    //next functions set the button that the user pressed on the virtual keyboard:

    private string setUsingRightHand(string pressedFinger)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || pressedInput == 1)
        {
            pressedFinger = "4";
            //keyboard.SetInteger("bluePressed", 1);
            bb.localPosition = bbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || pressedInput == 2)
        {
            pressedFinger = "3";
            //keyboard.SetInteger("yellowPressed", 1);
            yb.localPosition = ybDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || pressedInput == 3)
        {
            pressedFinger = "2";
            //keyboard.SetInteger("greenPressed", 1);
            gb.localPosition = gbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || pressedInput == 4)
        {
            pressedFinger = "1";
            //keyboard.SetInteger("redPressed", 1);
            rb.localPosition = rbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }

        return pressedFinger;
    }

    private string setUsingLeftHand(string pressedFinger)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || pressedInput == 1)
        {
            pressedFinger = "1";
            //keyboard.SetInteger("bluePressed", 1);
            bb.localPosition = bbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) || pressedInput == 2)
        {
            pressedFinger = "2";
            //keyboard.SetInteger("yellowPressed", 1);
            yb.localPosition = ybDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) || pressedInput == 3)
        {
            pressedFinger = "3";
            //keyboard.SetInteger("greenPressed", 1);
            gb.localPosition = gbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) || pressedInput == 4)
        {
            pressedFinger = "4";
            //keyboard.SetInteger("redPressed", 1);
            rb.localPosition = rbDown;
            cooldown = BUTTON_UP_COOLDOWN;
        }

        return pressedFinger;
    }

    private void SetUpButtonsOf()
    {
        if (cooldown > 0.0f)
            return;

        pressedInput = 0;
        if (Input.GetKeyUp(KeyCode.Alpha1) || !(Input.GetKeyDown(KeyCode.Alpha1)))
        {
            //keyboard.SetInteger("bluePressed", 0);
            bb.localPosition = bbUp;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) || !(Input.GetKeyDown(KeyCode.Alpha2)))
        {
            //keyboard.SetInteger("yellowPressed", 0);
            yb.localPosition = ybUp;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3) || !(Input.GetKeyDown(KeyCode.Alpha3)))
        {
            //keyboard.SetInteger("greenPressed", 0);
            gb.localPosition = gbUp;
        }
        if (Input.GetKeyUp(KeyCode.Alpha4) || !(Input.GetKeyDown(KeyCode.Alpha4)))
        {
            //keyboard.SetInteger("redPressed", 0);
            rb.localPosition = rbUp;
        }
    }

    private void SetAllButtonsOf()
    {
        if (cooldown > 0.0f)
            return;

        pressedInput = 0;
        //keyboard.SetInteger("bluePressed", 0);
        //keyboard.SetInteger("yellowPressed", 0);   
        //keyboard.SetInteger("greenPressed", 0);
        //keyboard.SetInteger("redPressed", 0);
        bb.localPosition = bbUp;
        yb.localPosition = ybUp;
        gb.localPosition = gbUp;
        rb.localPosition = rbUp;
    }
}
