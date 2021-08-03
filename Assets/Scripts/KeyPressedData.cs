using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JasHandExperiment
{
    /// <summary>
    /// IHandData impelemntation for hand movement directed from a key press. releven to simulation experiments
    /// </summary>
    public class KeyPressedData : IHandData
    {
        #region Data Members
        /// <summary>
        /// current key pressed
        /// </summary>
        private string mKeyPressed;

        /// <summary>
        /// the time samp of the key press.
        /// </summary>
        private string mTimeStamp;

        /// <summary>
        /// indicates wheter we need to flip read key pressed.
        /// </summary>
        private bool mShouldFlip;
        #endregion

        #region Properties
        /// <summary>
        /// property for the key that was pressed
        /// </summary>
        public string KeyPressed { get { return mKeyPressed; } }

        /// <summary>
        /// property for the time stamp of the key the was pressed
        /// </summary>
        public string TimeStamp { get { return mTimeStamp; } }
        #endregion

        #region Ctors
        /// <summary>
        /// creates an instance with first key pressed = ""
        /// </summary>
        public KeyPressedData()
        {
            mKeyPressed = string.Empty;
            mShouldFlip =  ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == Configuration.HandType.Right &&
                ConfigurationManager.Instance.Configuration.ExperimentType != Configuration.ExperimentType.Active;
        }

        /// <summary>
        /// creates an instance with initial key pressed
        /// </summary>
        /// <param name="keyPressed">the initial key pressed</param>
        public KeyPressedData(string keyPressed, string timeStamp)
        {
            mShouldFlip = ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == Configuration.HandType.Right &&
                ConfigurationManager.Instance.Configuration.ExperimentType != Configuration.ExperimentType.Active;

            mKeyPressed = keyPressed;
            mTimeStamp = timeStamp;
        }
        #endregion

        #region Functions

        /// <summary>
        /// the fucntino clones given instance of the class
        /// </summary>
        /// <returns>a clone f the instnace</returns>
        public object Clone()
        {
            KeyPressedData clone = new KeyPressedData(mKeyPressed,mTimeStamp);
            return clone;
        }

        /// <summary>
        /// implementation of filling the instance by settign key pressed
        /// </summary>
        /// <param name="data">the data from which to determine which key was pressed</param>
        public void SetHandMovementData(object data)
        {
            Tuple<string, string> timeKey = data as Tuple<string, string>;
            if (timeKey == null)
            {
                Debug.Log("Error casting KeyPressedData data to a Tuple of string,string (time,key)");
            }

            mTimeStamp = timeKey.Item1;
            mKeyPressed = timeKey.Item2;
            if (mShouldFlip)
            {
                mKeyPressed = (5 - int.Parse(mKeyPressed)).ToString();
            }
        }
        #endregion
    }
}
