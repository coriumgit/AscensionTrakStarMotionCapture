using CommonTools;
using FDTGloveUltraCSharpWrapper;
using JasHandExperiment.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using static HandController;
using static JasHandExperiment.ExperimentManager;
using ATC3DGWrapper;

namespace JasHandExperiment
{
    /// <summary>
    /// The class represents the device that reads from 5DT glove.
    /// the clas writes read data to a CSV file
    /// </summary>
    public class GlovesDevice : IHandMovementDevice
    {
        #region Constants
        
        /// <summary>
        /// the name of the port glove port to connect to.
        /// </summary>
        private const string GLOVE_PORT_NAME = "USB0";
        
        #endregion

        #region Data Members        
        /// <summary>
        /// the current coordinates of the hand according to the glove sensors
        /// </summary>
        HandCoordinatesData mCoordinates;

        /// <summary>
        /// the file to write the data to
        /// </summary>
        CSVFile mWriteFile;
        
        private DateTime logStartTime;
        bool doLogging = false;
        #endregion

        #region Functions
        /// <summary>
        /// see interface for documentation
        /// </summary>
        public bool Open()
        {
            ATC3DG.BIRD_ERROR_CODES err;
            if ((err = ATC3DG.init()) != ATC3DG.BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
            {
                Debug.Log("3D Guidance system initialization failed !");
                return false;
            }

            mCoordinates = new HandCoordinatesData();
            if (CalibrationManager.Mode == HandPlayMode.RealTime)
            {
                // write sensors data to file only on real time
                mWriteFile = new CSVFile();
                string path = CommonUtilities.GetParticipantCSVFileName(ConfigurationManager.Instance.Configuration.OutputFilesConfiguration.GloveMovementLogPath);
                var columns = CommonUtilities.CreateGlovesDataFileColumns(ConfigurationManager.Instance.Configuration.ExperimentType);
                var settings = new BatchCSVRWSettings();
                settings.WriteBatchSize = 1000;
                // interval?
                settings.WriteBatchDelayMsec = 1000 * 5;
                // init the file to write to
                mWriteFile.Init(System.Environment.CurrentDirectory + path, FileMode.Create, ',', columns, settings);
            }            

            return true;
        }

        /// <summary>
        /// see interface for documentation
        /// </summary>
        public void Close()
        {
            ATC3DG.closeBIRDSystem();          
            if (mWriteFile != null)            
                mWriteFile.Close();            
        }

        /// <summary>
        /// see interface for documentation
        /// </summary>
        public IHandData GetHandData()
        {
            float[] scaledSensors;
            // TODO: Extract here the Guidance Sensors data 
            ATC3DG.getAsynchronousData(out scaledSensors);

            // set current state            
            mCoordinates.SetHandMovementData(scaledSensors);
            if (CalibrationManager.Mode == HandPlayMode.RealTime && doLogging)
            {
                WriteCoordinatesToFile(scaledSensors);
            }
            
            return mCoordinates;
        }

        public float[] getSensorData(GuidanceSensor sensor)
        {
            float[] sensorData;
            // TODO: Extract here the Guidance Sensors data             
            if (ATC3DG.getAsynchronousDataSingleSensor((ushort)sensor, out sensorData) != ATC3DG.BIRD_ERROR_CODES.BIRD_ERROR_SUCCESS)
            {
                Debug.Log("3D Guidance system initialization failed !");
                return null;
            }
            else
                return sensorData;
        }

        public void startLogging()
        {
            doLogging = true;
            logStartTime = DateTime.Now;
        }

        public void pauseLogging()
        {
            doLogging = false;
        }

        public void resumeLogging()
        {
            doLogging = true;            
        }

        /// <summary>
        /// The function performs in place scaling of sensors
        /// </summary>
        /// <param name="rawSensors">the raw sensor values</param>
        /// <param name="max">the maximum sensor value of snsors to scale according to</param>
        /// <param name="min">the minimum sensor value of snsors to scale according to</param>
        private float[] ScaleRawData(ushort[] rawSensors, ushort[] max, ushort[] min)
        {
            float[] scaledSensors = new float[rawSensors.Length];
            for (int i = 0; i < rawSensors.Length; i++)
            {
                float denom = (max[i] - min[i]);
                int nominator = (rawSensors[i] - min[i]);
                if (denom == 0 || nominator < 0)
                {
                    scaledSensors[i] = 0;
                    continue;
                }

                scaledSensors[i] = (nominator / denom);
            }
            return scaledSensors;
        }

        /// <summary>
        /// The function gets timestamp and sensors and writes them to file (by mWriterFile)
        /// </summary>
        /// <param name="scaledSensors">the current sensors to write to file</param>        
        private void WriteCoordinatesToFile(float[] scaledSensors)
        {            
            string[] line = new string[scaledSensors.Length];
            int valueIndex = 0;            
            foreach (var sensorValue in scaledSensors)
            {
                if ((valueIndex + 2) % 8 == 0)
                    line[valueIndex] = (DateTime.Now - logStartTime).ToString();
                else
                    line[valueIndex] = sensorValue.ToString();
                valueIndex++;
            }
            //write to file
            mWriteFile.WriteLine(line);
        }
        
        #endregion
    }
}