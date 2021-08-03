using CommonTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ATC3DGWrapper;

namespace JasHandExperiment
{
    /// <summary>
    /// the class is an implementation of IHandData, gives data about each bone in a finger in the hand.
    /// suitable to 5DT glove sensors data.
    /// inherites from Dictionary, in order to save inner state for each finger (FingerType),
    /// and in each finger to each bone (float[] each cell is a bone).
    /// </summary>
    public class HandCoordinatesData : Dictionary<GuidanceSensor, float[]>, IHandData
    {
        #region Data Members
        /// <summary>
        /// current sensors information
        /// </summary>
        float[] mCurrentSensorsScaled;

        /// <summary>
        /// The time stamp of current coordinates
        /// </summary>
        public string TimeStamp { get; internal set; }

        /// <summary>
        /// The quality of current coordinates
        /// </summary>
        public float Quality { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// creates the structure with initialized member values
        /// </summary>
        public HandCoordinatesData()
        {
            float[] sensors = new float[CommonConstants.SCALED_SENSORS_ARRAY_LENGTH];
            TimeStamp = DateTime.Now.ToLongTimeString();
            InitDict(sensors);
        }

        /// <summary>
        /// creates an instance with given hand movement data
        /// </summary>
        /// <param name="scaledSensors">the initial hand data</param>
        public HandCoordinatesData(float[] scaledSensors)
        {
            TimeStamp = DateTime.Now.ToLongTimeString();
            InitDict(scaledSensors);
        }

        /// <summary>
        /// creates an instance with given hand movement data
        /// </summary>
        /// <param name="scaledSensors">the initial hand data - string flavour</param>
        public HandCoordinatesData(string[] scaledSensors)
        {
            TimeStamp = DateTime.Now.ToLongTimeString();
            // cast and launch
            float[] scaledSensorsFloat = StringUtilities.StringArrayToFloatArray(scaledSensors);
            InitDict(scaledSensorsFloat);
        } 
        #endregion

        #region Functions
        /// <summary>
        /// the functino convers string array to float array
        /// </summary>
        /// <param name="scaledSensors"></param>
        /// <returns></returns>
        
        /// <summary>
        /// the fucntino initialzied the inner dictionary by given params
        /// </summary>
        /// <param name="scaledSensors">the hand data for each bone and finger to set</param>
        private void InitDict(float[] scaledSensors)
        {
            mCurrentSensorsScaled = scaledSensors;
            // add each finger and then value per bone
            Array GuidanceSensors = Enum.GetValues(typeof(GuidanceSensor));
            foreach (GuidanceSensor guidanceSensor in GuidanceSensors) {
                float[] dataVec = new float[ATC3DG.DATA_PONITS_NR];
                for (int dataPointIdx = 0; dataPointIdx < ATC3DG.DATA_PONITS_NR; dataPointIdx++)
                    dataVec[dataPointIdx] = scaledSensors[(int)guidanceSensor * ATC3DG.DATA_PONITS_NR + dataPointIdx];
                Add(guidanceSensor, dataVec);                
            }           
        }

        /// <summary>
        /// the function updates the values of data for each bone and finger according to given data.
        /// </summary>
        /// <param name="scaledSensors">hand data to update - string flavour</param>
        private void UpdateValues(string[] scaledSensors, ushort[] max = null, ushort[] min = null)
        {
            // cast and launch
            float[] scaledSensorsFloat = StringUtilities.StringArrayToFloatArray(scaledSensors);
            UpdateValues(scaledSensorsFloat,max,min);
        }
        
        /// <summary>
        /// the function updates the values of data for each bone and finger according to given data.
        /// </summary>
        /// <param name="sensorsData">hand data to update</param>
        private void UpdateValues(float[] sensorsData, ushort[] max = null, ushort[] min = null)
        {
            mCurrentSensorsScaled = sensorsData;
            // sets per each bone and finger the current scaled sensor hand data   
            Array GuidanceSensors = Enum.GetValues(typeof(GuidanceSensor));
            foreach (GuidanceSensor guidanceSensor in GuidanceSensors) {
                for (int dataPointIdx = 0; dataPointIdx < ATC3DG.DATA_PONITS_NR; dataPointIdx++) 
                    this[guidanceSensor][dataPointIdx] = sensorsData[(int)guidanceSensor*ATC3DG.DATA_PONITS_NR + dataPointIdx];                                                              
            }

            TimeStamp = DateTime.Now.ToLongTimeString();
        }

        /// <summary>
        /// the fucntion clonescurrent instance
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            HandCoordinatesData copy = new HandCoordinatesData(mCurrentSensorsScaled);
            copy.TimeStamp = this.TimeStamp;
            return copy;
        }

        /// <summary>
        /// the function sets inner state according to data
        /// </summary>
        /// <param name="data">the data about hand movements to set inner state from</param>
        public void SetHandMovementData(object data)
        {
            if (data is string[])
            {
                UpdateValues(data as string[], CalibrationManager.UpperCalibValues, CalibrationManager.LowerCalibValues);
            }
            else if (data is float[])
            {
                UpdateValues(data as float[], CalibrationManager.UpperCalibValues, CalibrationManager.LowerCalibValues);
            }
            else
            {
                Debug.Log("hand movement data is not a the correct format (not in a suitable type)");
            }
        } 
        #endregion
    }
}
