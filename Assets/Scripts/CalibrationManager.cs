using CommonTools;
using FDTGloveUltraCSharpWrapper;
using JasHandExperiment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static HandController;
using static JasHandExperiment.ExperimentManager;

namespace JasHandExperiment
{
    public static class CalibrationManager
    {
        #region Constants
        private const string DEFAULT_CALIB_FILE_PATH = "calib.cal";

        private const string CALIBRATION_FILE_MAX_SEP = " MAX:";
        private const string CALIBRATION_FILE_LINE_FORMAT = "S{0}: MIN:{1} MAX:{2}";

        #endregion

        #region Data Members

        /// <summary>
        /// reference to the flove instance
        /// </summary>
        private static CfdGlove mGlove;
        
        /// <summary>
        /// did we initialize the static instance yet?
        /// </summary>
        private static bool mIsInitialized;

        /// <summary>
        /// the mode of the hand : calibration or realtime.
        /// </summary>
        private static HandPlayMode mMode;

        /// <summary>
        /// member indicating whether we are in auto calibration mode
        /// </summary>
        internal static bool isAutoCalibrating { get; set; }

        /// <summary>
        /// holds the min max calculators for calibration, if needed
        /// </summary>
        private static IMinMaxCalculator<ushort>[] mMiMaxCalcs;

        /// <summary>
        /// array of calibration values of MAX sensor value
        /// </summary>
        private static ushort[] mUpperCalibrationArray;

        /// <summary>
        /// array of calibration values of MIN sensor value
        /// </summary>
        private static ushort[] mLowerCalibrationArray;

        private static bool mIsFingerCalibration;
        #endregion

        #region Props
        
        /// <summary>
        /// property for upper sensor values used for calibration
        /// </summary>
        public static ushort[] UpperCalibValues { get { return mUpperCalibrationArray; } }
        
        /// <summary>
        /// property for lower sensor values used for calibration
        /// </summary>
        public static ushort[] LowerCalibValues { get { return mLowerCalibrationArray; } }

        /// <summary>
        /// member indicating which finger are wi calibrating now, nullable cause we don't need to calibrate for each finger and for bug fix
        /// </summary>
        public static GuidanceSensor? CurrentFinger;
        
        /// <summary>
        /// the mode of play, are we calibratiing or in actual experiment?
        /// </summary>
        public static HandPlayMode Mode { get { return mMode; } set { mMode = value; } }
        #endregion
        
        #region Functions

        /// <summary>
        /// the function initializes the calibration manager accordign to glove and mode of play
        /// </summary>
        /// <param name="gloveInst">the flove from which we get sensor values</param>
        /// <param name="mode">the mode of play, if it's undefined the function will not set mode</param>
        public static void Init(CfdGlove gloveInst, HandPlayMode mode = HandPlayMode.Undefined)
        {
            mIsFingerCalibration = false;
            CurrentFinger = null;
            mGlove = gloveInst;
            if (mode != HandPlayMode.Undefined)
            {
                mMode = mode;
            }
            if (mIsInitialized)
            {
                // already initialized don't re init.. 
                return;
            }
            mIsInitialized = true;
            if (mMode == HandPlayMode.RealTime)
            {
                var calibrationVals = ReadCalibrationFromFile(DEFAULT_CALIB_FILE_PATH);
                mUpperCalibrationArray = calibrationVals.Item1;
                mLowerCalibrationArray = calibrationVals.Item2;
            }
            else
            {
                ResetUpperAndLowerVals();
            }
        }
        
        /// <summary>
        /// The function sets min and max values to relevant scaling UPPER and LOWER values
        /// </summary>
        private static void ResetUpperAndLowerVals()
        {
            // for calibration set MAX and MIN vals to avoid conflicting 
            mUpperCalibrationArray = new ushort[CommonConstants.SCALED_SENSORS_ARRAY_LENGTH];
            mLowerCalibrationArray = new ushort[CommonConstants.SCALED_SENSORS_ARRAY_LENGTH];
            for (int i = 0; i < mUpperCalibrationArray.Length; i++)
            {
                mUpperCalibrationArray[i] = ushort.MinValue;
                mLowerCalibrationArray[i] = ushort.MaxValue;
            }
        }

        /// <summary>
        /// the function handles user input to manipulate calibration states
        /// if user pressed A - read calibration from file and apply it
        /// if user pressed R - reset calibration and apply it
        /// if user pressed G - write current calibration to file
        /// if user pressed M - start auto calibrating
        /// if user pressed S - just set the auto calibrated values 
        /// if user pressed space - moving on from calibration scene to next scene
        /// </summary>
        internal static void HandCalibrationUserInput()
        {
            // if user pressed A - read calibration from file and apply it
            if (Input.GetKeyDown(KeyCode.A))
            {
                var calib = ReadCalibrationFromFile(DEFAULT_CALIB_FILE_PATH);
                mUpperCalibrationArray = calib.Item1;
                mLowerCalibrationArray = calib.Item2;
                mGlove.SetCalibrationAll(mUpperCalibrationArray, mLowerCalibrationArray);
            }
            // if user pressed R - reset calibration and apply it
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetUpperAndLowerVals();
                SetCalibration();
            }
            if (mIsFingerCalibration == true)
            {
                UpdateUpperLowerValues(CurrentFinger);
            }
            // if user pressed R - reset calibration and apply it
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (!mIsFingerCalibration)
                {
                    mGlove.ResetCalibration();
                }
                if (CurrentFinger == null && mIsFingerCalibration == false)
                {
                    CurrentFinger = GuidanceSensor.INDEX_PROX;
                    mIsFingerCalibration = true;
                    return;
                }
                //if (CurrentFinger == GuidanceSensors.THUMB)
                //{
                 //   return;
                //}

                CurrentFinger = (GuidanceSensor)((int)CurrentFinger + 1);
            }
            // if user pressed G - write current calibration to file
            if (Input.GetKeyDown(KeyCode.G))
            {
                WriteCalibrationValuesToFile(DEFAULT_CALIB_FILE_PATH);

                isAutoCalibrating = false;
            }
            // if user pressed M - start auto calibrating
            if (Input.GetKeyDown(KeyCode.M) || isAutoCalibrating)
            {
                if (!isAutoCalibrating)
                {
                    mGlove.ResetCalibration();
                }
                isAutoCalibrating = true;
                UpdateUpperLowerValues();
                mGlove.SetCalibrationAll(mUpperCalibrationArray,mLowerCalibrationArray);
            }

            // if user pressed S - just set the auto calibrated values, 
            // not useful since all other calibration setting functions calls SetCurrentCalibrationValues as well
            if (Input.GetKeyDown(KeyCode.S))
            {
                mGlove.SetCalibrationAll(mUpperCalibrationArray, mLowerCalibrationArray);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CommonConstants.FirstRun = false;
                SceneManager.LoadScene("restScene");

            }
        }

        /// <summary>
        /// The function updates calibration values to be later on used for scaling hand movement
        /// </summary>
        /// <param name="currentFinger">current finger to scale, nullable - if null the function will fail</param>
        /// <param name="rawSensors">all of the sesnor values</param>
        private static void UpdateUpperLowerValues(GuidanceSensor? currentFinger, ushort[] rawSensors = null)
        {
            if (currentFinger == null)
            {
                Debug.Log("CURERNT FINGER IS NULL!!");
            }
            if (rawSensors == null)
            {
                rawSensors = new ushort[CommonConstants.SCALED_SENSORS_ARRAY_LENGTH];
                mGlove.GetSensorRawAll(ref rawSensors);
            }

            // due to the array indexes we apply some mathematical calc to point out to right finger
            int fingerOffset = 3 * ((int)CurrentFinger +1);

            //if (CurrentFinger == GuidanceSensors.THUMB)
            //{
            //    fingerOffset = 0;
            //}
            for (int i = 0; i < 2; i++)
            {
                if (rawSensors[fingerOffset + i] > mUpperCalibrationArray[fingerOffset + 1])
                {
                    mUpperCalibrationArray[fingerOffset + 1] = rawSensors[fingerOffset + 1];
                }
                if (rawSensors[fingerOffset +1] < mLowerCalibrationArray[fingerOffset + 1])
                {
                    mLowerCalibrationArray[fingerOffset + 1] = rawSensors[fingerOffset + 1];
                }
            }
            
        }

        /// <summary>
        /// the functin reads calibration of glove device from file and returns it's values.
        /// </summary>
        /// <param name="fileName">the file to read from</param>
        /// <returns>Tuple where first element is upper values array and second otem is lower values arrray</returns>
        internal static Tuple<ushort[], ushort[]> ReadCalibrationFromFile(string fileName)
        {
            CSVFile calibFile = new CSVFile();
            calibFile.Init(new FileStream(fileName, FileMode.Open), ':', null, null);
            var lines = calibFile.ReadLines();
            List<ushort> upperVals = new List<ushort>();
            List<ushort> lowerVals = new List<ushort>();
            foreach (var line in lines)
            {
                // find space to split the line item
                int indexOfSpace = line[line.Length - 2].IndexOf(" ");
                string lowerStr = line[line.Length - 2].Substring(0, indexOfSpace);
                ushort upper = ushort.Parse(line[line.Length - 1]);
                ushort lower = ushort.Parse(lowerStr);

                upperVals.Add(upper);
                lowerVals.Add(lower);
            }
            calibFile.Close();

            return new Tuple<ushort[], ushort[]>(upperVals.ToArray(), lowerVals.ToArray());
        }

        /// <summary>
        /// the ffunction writes current upper and lower values for calibration to file
        /// </summary>
        /// <param name="fileName">path to write the file to</param>
        internal static void WriteCalibrationValuesToFile(string fileName)
        {
            if (mUpperCalibrationArray == null || mLowerCalibrationArray == null)
            {
                Debug.Log("cannot write calibration file, lower and/or upper values are null");
            }

            CSVFile writeCalib = new CSVFile();
            writeCalib.Init(new FileStream(fileName, FileMode.Create), CALIBRATION_FILE_MAX_SEP, null);
            for (int i = 0; i < mUpperCalibrationArray.Length; i++)
            {
                writeCalib.WriteLine(string.Format(CALIBRATION_FILE_LINE_FORMAT, i, mLowerCalibrationArray[i], mUpperCalibrationArray[i]));
            }
            writeCalib.Close();
        }

        /// <summary>
        /// the function sets upper and ower sensor scalibration.
        /// </summary>
        internal static void SetCalibration()
        {
            mGlove.SetCalibrationAll(mUpperCalibrationArray, mLowerCalibrationArray);
        }

        /// <summary>
        /// The functino gets raw values from the glove device and updates lower and upper calibration arrays accordingly.
        /// </summary>
        internal static void UpdateUpperLowerValues(ushort[] rawSensors = null)
        {
            if (rawSensors == null)
            {
                rawSensors = new ushort[CommonConstants.SCALED_SENSORS_ARRAY_LENGTH];
                mGlove.GetSensorRawAll(ref rawSensors);
            }
            
            for (int i = 0; i < rawSensors.Length; i++)
            {
                if (rawSensors[i] > mUpperCalibrationArray[i])
                {
                    mUpperCalibrationArray[i] = rawSensors[i];
                }
                if (rawSensors[i] < mLowerCalibrationArray[i])
                {
                    mLowerCalibrationArray[i] = rawSensors[i];
                }
            }
        }

        #endregion
    }
}
