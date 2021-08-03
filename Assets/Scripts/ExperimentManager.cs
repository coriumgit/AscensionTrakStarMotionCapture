using JasHandExperiment.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ATC3DGWrapper;
//using JasHandExperiment.Configuration;
using UnityEngine.UI;
using CommonTools;

namespace JasHandExperiment
{
    public class ExperimentManager : MonoBehaviour
    {
        #region Data Members

        private static ExperimentManager sInstance;

        private enum State { STAND_BY, RUNNING, PAUSED, ENDED }

        /// <summary>
        /// The experiment runtime singletone instance
        /// </summary>
        ExperimentRuntime mExperimentRuntime;

        /// <summary>
        /// The experimental phase we are currently on
        /// </summary>
        State state = State.STAND_BY;

        /// <summary>
        /// prefab for female hands
        /// </summary>
        public GameObject FemaleHandPrefab;

        /// <summary>
        /// prefab for female hands
        /// </summary>
        public GameObject MaleHandPrefab;

        /// <summary>
        /// prefab for female hands
        /// </summary>
        // public GameObject RightKeyboardPrefab;
        public GameObject RightKeyboardPrefabFemale;

        public GameObject RightKeyboardPrefabMale;
        /// <summary>
        /// prefab for female hands
        /// </summary>
        //public GameObject LeftKeyboardPrefab;
        public GameObject LeftKeyboardPrefabFemale;

        public GameObject LeftKeyboardPrefabMale;

        public int maxStarsNr;

        private BaseConditionedDynamicObjectCreator<GenderType> mDynamicHandCreator;

        private BaseConditionedDynamicObjectCreator<HandType> mDynamicKeyboardCreator;

        /// <summary>
        /// indicates if we are in calibration mode or real time running
        /// </summary>
        public HandPlayMode Mode;

        private HandController handController;
        public Text squence;
        public Text stars;
        CSVFile mReadFile;        
        private float timer;
        private GameObject msgsBg;
        private Text msgsDisplay;
        private Text timeDisplay;
        private string lastDTUpdated = System.DateTime.MinValue.ToLongTimeString();        
        private int startsNr;
        private uint blockIdx = 0;
        private string pauseMsgBase;        
        //maxCounterBound : the max number of stars to show on screen

        private KeyBoardConroller keyboardController;

        private bool wasBeepPlayed = false;
        #endregion

        void Awake()
        {           
            if (sInstance != null && sInstance != this)
            {
                Destroy(gameObject);
            }
            else if (sInstance != null)
            {
                sInstance = this;
                DontDestroyOnLoad(gameObject);
            }

            msgsBg = GameObject.Find("msgsBg");
            msgsBg.SetActive(false);
            msgsDisplay = GameObject.Find("msgsDisplay").GetComponent<Text>();
            timeDisplay = GameObject.Find("experimenterMsgs").GetComponent<Text>();
            GameObject.Find("Squence").GetComponent<Text>().text = ConfigurationManager.Instance.Configuration.Squence;
            pauseMsgBase = "Rest for " + ConfigurationManager.Instance.Configuration.SubRuns[0].InterBlockTimeout.ToString() + " seconds";
        }

        // Use this for initialization
        void Start()
        {            
            sInstance = this;

            mExperimentRuntime = ExperimentRuntime.Instance;
            CalibrationManager.Mode = Mode;

            if (MaleHandPrefab == null || FemaleHandPrefab == null) {
                Debug.Log("Experiment Manager wasn't initialized with male or female hands");
                Debug.Log("Due to uninitialized ExperimentManager , hand gameobjects will not be created");

                return;
            }
            
            mDynamicHandCreator = new BaseConditionedDynamicObjectCreator<GenderType>(MaleHandPrefab, FemaleHandPrefab);            
            GenderType gender = ConfigurationManager.Instance.Configuration.ParticipantConfiguration.Gender;
            GameObject handObj = Instantiate(mDynamicHandCreator.GetObjectToCreate(x => x.Equals(GenderType.Male), gender));//.GetComponent<HandController>();                        

            if (LeftKeyboardPrefabFemale == null || RightKeyboardPrefabFemale == null || LeftKeyboardPrefabMale == null || RightKeyboardPrefabMale == null)
            {
                if (SceneManager.GetActiveScene().name.Equals("testroom"))
                {
                    Debug.Log("One of Keyboard prefabs is not initialized in ExperimentManager, init it if it is needed");
                    Debug.Log("Due to uninitialized ExperimentManager,  keyboard gameobject will not created");
                }

                return;
            }

            if (gender == GenderType.Female)
            {
                // create and init keyboard creator
                mDynamicKeyboardCreator = new BaseConditionedDynamicObjectCreator<HandType>(LeftKeyboardPrefabFemale, RightKeyboardPrefabFemale);

            }
            if (gender == GenderType.Male)
            {
                // create and init keyboard creator
                mDynamicKeyboardCreator = new BaseConditionedDynamicObjectCreator<HandType>(LeftKeyboardPrefabMale, RightKeyboardPrefabMale);

            }
            HandType side = ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate;
            Instantiate(mDynamicKeyboardCreator.GetObjectToCreate(x => x.Equals(HandType.Left), side));
            if (side == HandType.Left)
                handController = handObj.GetComponentsInChildren<HandController>()[0];
            else //side == HandType.Right
                handController = handObj.GetComponentsInChildren<HandController>()[1];
             
             keyboardController = GameObject.FindGameObjectWithTag("ResponseBox").GetComponent<KeyBoardConroller>();            
        }

        void Update()
        {
            switch (state)
            {
                case State.RUNNING:
                    timer -= Time.deltaTime;
                    if (timer > 0)
                    {
                        switch (ConfigurationManager.Instance.Configuration.ExperimentType)
                        {
                            case ExperimentType.Active:
                                if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) ||
                                     Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4)) &&
                                     (startsNr < maxStarsNr))
                                {
                                    startsNr++;
                                    stars.text += "*";
                                }

                                break;
                            case ExperimentType.PassiveSimulation:
                            case ExperimentType.PassiveWatchingReplay:
                                KeyPressedData key = keyboardController.getKeyPressedData();
                                if (key != null && !string.IsNullOrEmpty(key.KeyPressed) && !key.TimeStamp.Equals(lastDTUpdated))
                                {
                                    lastDTUpdated = key.TimeStamp;
                                    stars.text += "*";                                    
                                }
                                else if (key == null)
                                    moveToPausedState();

                                break;
                        }                        
                    }
                    else
                    {
                        if (blockIdx < ConfigurationManager.Instance.Configuration.SubRuns[0].BlocksAmount - 1)                        
                            moveToPausedState();                        
                        else
                        {
                            CommonConstants.EndOfExperement = true;
                            state = State.ENDED;
                            msgsBg.SetActive(true);
                            msgsDisplay.gameObject.SetActive(true);                            
                            msgsDisplay.text = "End of experiment";
                            timeDisplay.gameObject.SetActive(true);
                            timeDisplay.text = "End of experiment.";
                            //handController.pauseLogging();
                            keyboardController.pauseLogging();
                        }                            
                    }

                    break;
                case State.PAUSED:
                    timer -= Time.deltaTime;
                    if (timer <= 0)
                    {                                                
                        state = State.STAND_BY;
                        if (!handController.IsCalibrating)                                                                               
                            resumeTrial();      
                        else
                            timeDisplay.gameObject.SetActive(false);
                    }
                    else
                    {
                        timeDisplay.text = "End of Block #" + blockIdx + "\nBlock #" + (blockIdx + 1) + " starting in: " + timer.ToString("0.00");
                        if (!wasBeepPlayed && timer <= 5)
                        {
                            gameObject.GetComponent<AudioSource>().Play();
                            wasBeepPlayed = true;
                        }

                        if (ConfigurationManager.Instance.Configuration.ExperimentType == ExperimentType.Active && Input.GetKey(KeyCode.C))
                        {                            
                            handController.recalibrateDevice();
                        }
                    }
                    
                    break;
                case State.ENDED:
                    if (Input.GetKey(KeyCode.Return))
                    {
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit ();
#endif
                    }
                    break;
            }
        }

        public void resumeTrial()
        {
            if (state == State.STAND_BY)
            {
                msgsDisplay.gameObject.SetActive(false);
                timeDisplay.gameObject.SetActive(false);
                msgsBg.SetActive(false);
                state = State.RUNNING;
                stars.text = "";
                startsNr = 0;
                timer = ConfigurationManager.Instance.Configuration.SubRuns[0].BlockDuration;
                if (blockIdx == 0)
                {
                    handController.startLogging();
                    keyboardController.startLogging();
                }
                else
                {
                    handController.resumeLogging();
                    keyboardController.resumeLogging();
                }
            }
        }

        private void moveToPausedState()
        {
            timer = ConfigurationManager.Instance.Configuration.SubRuns[0].InterBlockTimeout;
            msgsBg.SetActive(true);
            msgsDisplay.gameObject.SetActive(true);
            msgsDisplay.text = pauseMsgBase;
            timeDisplay.gameObject.SetActive(true);
            blockIdx++;
            ExperimentRuntime.Instance.TrialNumber++;
            state = State.PAUSED;
            //handController.pauseLogging();
            keyboardController.pauseLogging();
        }  
    }    
}