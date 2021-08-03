using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommonTools;
using JasHandExperiment.Configuration;
using UnityEngine;

namespace JasHandExperiment
{
    #region Enums
    
    /// <summary>
    /// enum to differentiate betweeen calibration mode and realtime regular mode
    /// </summary>
    public enum HandPlayMode
    {
        RealTime,
        Calibration,
        Undefined
    }

    public enum FingerType
    {        
        INDEX = 0,
        MIDDLE = 1,
        RING = 2,
        LITTLE = 3        
    }

    public enum FingerSection
    {
        PROXIMAL = 0,        
        DISTAL = 1
    }   

    public enum EfdSensors
    {
        FD_THUMBNEAR = 0,
        FD_THUMBFAR = 1,
        FD_THUMBINDEX = 2,
        FD_INDEXNEAR = 3,
        FD_INDEXFAR = 4,
        FD_INDEXMIDDLE = 5,
        FD_MIDDLENEAR = 6,
        FD_MIDDLEFAR = 7,
        FD_MIDDLERING = 8,
        FD_RINGNEAR = 9,
        FD_RINGFAR = 10,
        FD_RINGLITTLE = 11,
        FD_LITTLENEAR = 12,
        FD_LITTLEFAR = 13,
        FD_THUMBPALM = 14,
        FD_WRISTBEND = 15,
        FD_PITCH = 16,
        FD_ROLL = 17
    }
    
    public enum GuidanceSensor
    {        
        INDEX_PROX = 0,
        INDEX_DIST = 1,
        MIDDLE_PROX = 2,
        MIDDLE_DIST = 3,
        RING_PROX = 4,
        RING_DIST = 5,
        LITTLE_PROX = 6,
        LITTLE_DIST = 7                          
    }

    public enum CalibrationPhase
    {
        ALIGNING_REAL_AND_VIRTUAL_HANDS = 0,
        ALIGNING_REAL_AND_VIRTUAL_RESPONSE_BOXES = 1,
        WAITING_ON_END_CALIBRATION_BTN = 2,
        NOT_CALIBRATING
    }

    public static class GuidenceSensorExtension        
    {
        public static FingerType toFinger(GuidanceSensor sensor)
        {
            return (FingerType)((int)sensor / 2);
        }

        public static FingerSection toSection(GuidanceSensor sensor)
        {
            return (FingerSection)((int)sensor % 2);
        }
    }    

    public enum KeyPressedColumn
    {
        Time = 0,
        Key = 1
    } 
    #endregion

    /// <summary>
    /// The class contains constant members that are relevant across the application
    /// </summary>
    public static class CommonConstants
    {
        public static int TrialNumber = 1;
        public static bool FirstRun = true;
        public static bool EndOfExperement = false;

        // file relevant consts
        public const char CSV_SEPERATOR = ',';
        public const int SCALED_SENSORS_ARRAY_LENGTH = 64;
        public const int TIME_COL_INDEX = 6;
        public const int TIME_COL_INDEX_KEYBOARD_DATA = 0;
        public const int KEY_PRESS_COL_INDEX = 1;
        public const char SESSION_TRIAL_SEPERATOR = '-';
        
        // hand prefab names
        internal const string MALE_HAND_RENDERER_CONTAINING_OBJECT_NAME = "fp_male_hand";
        internal  const string FEMALE_HAND_RENDERER_CONTAINING_OBJECT_NAME = "fp_female_hand";
        internal const string FEMALE_HAND_PREFAB = "FemaleHand";
        internal const string MALE_HAND_PREFAB = "MaleHand";

        // animation consts
        internal const string INDEX_KEY_PRESS_PARAM = "IndexPress";
        internal const string MIDDLE_KEY_PRESS_PARAM = "MiddlePress";
        internal const string RING_KEY_PRESS_PARAM = "RingPress";
        internal const string PINKY_KEY_PRESS_PARAM = "PinkyPress";

        // animation triggers consts
        internal const string INDEX_KEY_PRESS_STRING = "4";
        internal const string MIDDLE_KEY_PRESS_STRING = "3";
        internal const string RING_KEY_PRESS_STRING = "2";
        internal const string PINKY_KEY_PRESS_STRING = "1";

        // glove consts
        internal const string USB_PORT_NAME_PREFIX = "USB";
        internal const string DT_GLOVE_MANUFACTURER = "5DT";
        internal const string DT_RIGHT_GLOVE_INSTANCE_ID_PREFIX = "DG14UR";
        internal const string DT_LEFT_GLOVE_INSTANCE_ID_PREFIX = "DG14UL";

        // csv file extension
        internal const string CSV_EXTENSION = ".csv";

        internal const string AVATAR_STATE_EXTENSION = ".dat";
    }

    /// <summary>
    /// The clas contains common functions being used accross the application
    /// </summary>
    public static class CommonUtilities
    {
        private const string HAND_JOINT_OBJECT_NAME = "hand_joint";

        /// <summary>
        /// The function returns the relevant hand prefab according to gender
        /// </summary>
        /// <param name="gender">the relevant gender</param>
        /// <returns>name of the relevant prefab</returns>
        public static string GetHandPrefabName(GenderType gender)
        {
            if (gender == GenderType.Female)
            {
                return CommonConstants.FEMALE_HAND_PREFAB;
            }

            return CommonConstants.MALE_HAND_PREFAB;
        }

        /// <summary>
        /// The functino returns path 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static string GetParticipantCSVFileName(string directoryPath)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + directoryPath))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + directoryPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(directoryPath);
            sb.Append(ConfigurationManager.Instance.Configuration.ParticipantConfiguration.Number + @"\");
            // if subject directory not found...
            string subjectFolderPath = Environment.CurrentDirectory + sb.ToString();
            if (!Directory.Exists(subjectFolderPath))
            {
                Directory.CreateDirectory(subjectFolderPath);
            }
            sb.Append(ConfigurationManager.Instance.Configuration.SessionsConfiguration[ConfigurationManager.Instance.Configuration.SessionsConfiguration.Count -1].Number);
            sb.Append(CommonConstants.SESSION_TRIAL_SEPERATOR);
            sb.Append(CommonConstants.TrialNumber);
            sb.Append(CommonConstants.CSV_EXTENSION);
            return sb.ToString();
        }

        public static string GetParticipantAvatarStateFileName(string directoryPath)
        {
            if (!Directory.Exists(Environment.CurrentDirectory + directoryPath))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + directoryPath);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(directoryPath);
            sb.Append(ConfigurationManager.Instance.Configuration.ParticipantConfiguration.Number + @"\");
            // if subject directory not found...
            string subjectFolderPath = Environment.CurrentDirectory + sb.ToString();
            if (!Directory.Exists(subjectFolderPath))
            {
                Directory.CreateDirectory(subjectFolderPath);
            }
            sb.Append(ConfigurationManager.Instance.Configuration.SessionsConfiguration[ConfigurationManager.Instance.Configuration.SessionsConfiguration.Count - 1].Number);
            sb.Append(CommonConstants.SESSION_TRIAL_SEPERATOR);
            sb.Append(CommonConstants.TrialNumber);
            sb.Append(CommonConstants.AVATAR_STATE_EXTENSION);
            return sb.ToString();
        }

        /// <summary>
        /// The function returns the renderer name of the hand according to gender
        /// </summary>
        /// <param name="gender">the relevant gender</param>
        /// <returns>name of the relevant prefab</returns>
        public static string GetRendererParentObjectName(GenderType gender)
        {
            if (gender == GenderType.Female)
            {
                return CommonConstants.FEMALE_HAND_RENDERER_CONTAINING_OBJECT_NAME;
            }

            return CommonConstants.MALE_HAND_RENDERER_CONTAINING_OBJECT_NAME;
        }
        
        /// <summary>
        /// The function returns relevant columns for CSV file for each experiment type
        /// </summary>
        /// <param name="type">relevant experiment type</param>
        /// <returns>columns IEnumerable of all of the columns of the CSVFile relevant to the experiment type</returns>
        public static IEnumerable<string> CreateGlovesDataFileColumns(ExperimentType type)
        {
            List<string> cols;
            switch (type)
            {
                case ExperimentType.Active:
                case ExperimentType.PassiveWatchingReplay:
                    cols = new List<string>();//[ATC3DGWrapper.ATC3DG.DATA_PONITS_NR * Enum.GetValues(typeof(FingerType)).Length];
                    for (int fingerIdx = 1; fingerIdx <= Enum.GetValues(typeof(FingerType)).Length; fingerIdx++)
                    {
                        cols.Add("x" + (2*fingerIdx - 1));
                        cols.Add("y" + (2*fingerIdx - 1));
                        cols.Add("z" + (2*fingerIdx - 1));
                        cols.Add("a" + (2*fingerIdx - 1));
                        cols.Add("e" + (2*fingerIdx - 1));
                        cols.Add("r" + (2*fingerIdx - 1));
                        cols.Add("time" + (2*fingerIdx - 1));
                        cols.Add("quality" + (2*fingerIdx - 1));
                        cols.Add("x" + (2 * fingerIdx));
                        cols.Add("y" + (2 * fingerIdx));
                        cols.Add("z" + (2 * fingerIdx));
                        cols.Add("a" + (2 * fingerIdx));
                        cols.Add("e" + (2 * fingerIdx));
                        cols.Add("r" + (2 * fingerIdx));
                        cols.Add("time" + (2 * fingerIdx));
                        cols.Add("quality" + (2 * fingerIdx));
                    }
                     
                    break;
                case ExperimentType.PassiveSimulation:
                    //cols = new string[] { KeyPressedColumn.Time.ToString(), KeyPressedColumn.Key.ToString() };
                    cols = new List<string>();
                    break;
                default:
                    return null;
            }

            return cols;
        }

        /// <summary>
        /// The function wrapps usb utilities use to get glove side
        /// </summary>
        /// <returns>the side of the glove that is connected None if no glove is connected</returns>
        public static HandType GetGloveSide()
        {
            // get reqiuered usb dev id
            HandType gloveSide = HandType.None;
            var usbDevs = USBUtilities.GetConnectedDevices();
            for (int i=0; i< usbDevs.Count; i++)
            {
                // search for glove device
                if (usbDevs[i].Manufacturer.Equals(CommonConstants.DT_GLOVE_MANUFACTURER))
                {
                    if (usbDevs[i].InstanceID.Contains(CommonConstants.DT_RIGHT_GLOVE_INSTANCE_ID_PREFIX))
                    {
                        gloveSide = HandType.Right;
                    }
                    else if (usbDevs[i].InstanceID.Contains(CommonConstants.DT_LEFT_GLOVE_INSTANCE_ID_PREFIX))
                    {
                        gloveSide = HandType.Left;
                    }

                   break;
                }
            }
            
            return gloveSide;
        }

        /// <summary>
        /// The function searches for a specific finger's end effector object and returns it's Transform
        /// </summary>
        /// <param name="handController">the hand controller so search from</param>
        /// <param name="fingerSensor">the sensor whose end  we are looking for </param>
        /// <returns>Transform object of the end effector we were searching</returns>
        public static Transform GetEndEffector(Transform handController, GuidanceSensor fingerSensor)
        {
            Transform handObj = FindObjectWithName(handController, HAND_JOINT_OBJECT_NAME);
            //if (fingerSensor == GuidanceSensors.BACK_OF_PALM)
            //    return handJoint;
            //else
            Transform transformsIt = handObj.GetChild((int)fingerSensor);
            while (transformsIt.childCount > 0)            
                transformsIt = transformsIt.GetChild(0);            

            return transformsIt;
        }

        /// <summary>
        /// The function seached for a specific finger object and returns it's Transform
        /// </summary>
        /// <param name="handController">the hand controller so search from</param>
        /// <param name="type">the finger we search for</param>
        /// <returns>Transform object of the finger we were searching</returns>
        public static Transform GetFingerObject(Transform handController, GuidanceSensor fingerSensor)
        {
            var handJoint = FindObjectWithName(handController, HAND_JOINT_OBJECT_NAME);
            //if (fingerSensor == GuidanceSensors.BACK_OF_PALM)
            //    return handJoint;
            //else
                return handJoint.GetChild((int)fingerSensor);
        }

        /// <summary>
        /// extension method for a Transform object, the fucntino searches for a given game object below given parent game object.
        /// </summary>
        /// <param name="parent">from where to search</param>
        /// <param name="nameToSearch">nae of the game pbject we are searching</param>
        /// <returns>The transform of the game object we are searching. the function will return the first occurence of this name</returns>
        public static Transform FindObjectWithName(this Transform parent, string nameToSearch)
        {
            return GetChildObject(parent, nameToSearch);
        }

        /// <summary>
        /// the fucntino searches for a given game object below given parent game object.
        /// </summary>
        /// <param name="parent">from where to search</param>
        /// <param name="nameToSearch">nae of the game pbject we are searching</param>
        /// <returns>nul if we didn't find suitable game object, or otherwise, The transform of the game object we are searching. the function will return the first occurence of this name</returns>
        private static Transform GetChildObject(Transform parent, string nameToSearch)
        {
            // end of search
            if (parent == null)
            {
                return null;
            }

            // run on all child and continue searching
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (string.Equals(child.name, nameToSearch))
                {

                    // spread seach t children
                    return child;
                }
                if (child.childCount > 0)
                {
                    var suitableChild = GetChildObject(child, nameToSearch);
                    if (suitableChild != null)
                    {
                        // suitable match
                        return suitableChild;
                    }
                }
            }
            
            // out of luck
            return null;
        }

        /// <summary>
        /// the function figures out which key was pressed.
        /// </summary>
        /// <returns>the that was pressed, if no key was pressed KeyCode.None is returned</returns>
        public static KeyCode FetchKey()
        {
            var e = System.Enum.GetNames(typeof(KeyCode)).Length;
            for (int i = 0; i < e; i++)
            {
                if (Input.GetKey((KeyCode)i))
                {
                    return (KeyCode)i;
                }
            }

            return KeyCode.None;
        }
    }
}
