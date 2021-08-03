using CommonTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JasHandExperiment
{
    /// <summary>
    /// abstract calss the represents hand movement information from a file.
    /// implements IHandMovementDevice by assuming the data is from a file.
    /// </summary>
    public abstract class BaseHandMovementFileDevice : IHandMovementDevice
    {
        #region Data Members
        /// <summary>
        /// The name of the fie to read data from
        /// </summary>
        protected string mFileName;

        /// <summary>
        /// the CSVFile to read hand data from
        /// </summary>
        protected CSVFile mCSVFile;

        /// <summary>
        /// lock for synchronizatino between file reader thread and other functions.
        /// </summary>
        protected object mLock;

        /// <summary>
        /// saves current hand data
        /// </summary>
        protected IHandData mData;

        /// <summary>
        /// the columns of the CSVFile.
        /// </summary>
        protected IEnumerable<string> mColumns;

        #endregion

        #region Ctors
        public BaseHandMovementFileDevice()
        {
            mLock = new object();
        }

        #endregion

        #region Functions
        /// <summary>
        /// see interface for documentation
        /// </summary>
        public void Close()
        {
            if (mCSVFile != null)
            {
                mCSVFile.Close();
                mCSVFile = null;
            }
        }

        /// <summary>
        /// see interface for documentation
        /// </summary>
        public virtual IHandData GetHandData()
        {
            lock (mLock)
            {
                var clone = mData.Clone() as IHandData;
                // clone to avoid conflicts with other threads
                return clone;
            }
        }

        /// <summary>
        /// abstract function to get the relevant file name
        /// </summary>
        /// <returns>the path to the file to read from</returns>
        public abstract string GetFileName();

        /// <summary>
        /// The function creates a specific IHandData structure,
        /// that is relevant to this specific hand movement file device
        /// </summary>
        /// <returns>IDandData struncture with data to apply hand movement</returns>
        public abstract IHandData CreateHandMovementData();

        /// <summary>
        /// the function performs initialization bedore starting the device
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// the function performs initialization bedore starting the device
        /// </summary>
        public virtual IEnumerable<string> GetCSVColumns()
        {
            return CommonUtilities.CreateGlovesDataFileColumns(ConfigurationManager.Instance.Configuration.ExperimentType);
        }        

        /// <summary>
        /// see interface for documentation
        /// </summary>
        public bool Open()
        {
            if (mCSVFile != null)            
                return false;
            
            // save the file params, and open it
            mFileName = GetFileName();
            mData = CreateHandMovementData();
            mCSVFile = new CSVFile();
            BatchCSVRWSettings settings = GetReadSettings();
            try
            {
                mCSVFile.Init(mFileName, FileMode.Open, CommonConstants.CSV_SEPERATOR, GetCSVColumns(), settings);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
            if (settings.ReadBatchDelayMsec != BatchCSVRWSettings.DEFAULT_READ_DELAY_MSEC)
            {
                // subscribe to lines ready
                mCSVFile.OnLinesReady += OnLinesReady;
                // start reading from file
                mCSVFile.ReadLines();
            }

            Init();

            return true;
        }

        /// <summary>
        /// The function returns the settings for CSV file read.
        /// </summary>
        /// <returns></returns>
        public abstract BatchCSVRWSettings GetReadSettings();

        /// <summary>
        /// handler for lines ready event
        /// </summary>
        /// <param name="obj">the lines read from file</param>
        private void OnLinesReady(IEnumerable<string[]> obj)
        {
            lock (mLock)
            {
                // lines are ready, react to them according to implementation of absract function OnCoordinatesUpdate
                // we take first cause we read one line at a time in this device
                OnCoordinatesUpdate(obj.First());
            }
        }

        /// <summary>
        /// virutal function with basic lines ready reaction
        /// </summary>
        /// <param name="lines">the lines read from file</param>
        protected abstract void OnCoordinatesUpdate(string[] lines);

        public float[] getSensorData(GuidanceSensor sensor)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
