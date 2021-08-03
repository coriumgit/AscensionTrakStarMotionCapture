using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommonTools
{
    /// <summary>
    /// the class reads from a CSVFile only if time for the line is reached. should enable synchronized read from multiple readers.
    /// </summary>
    public class TimedCSVReader
    {
        #region Data Members

        /// <summary>
        /// The CSVFile to read from.
        /// </summary>
        private CSVFile mFile;

        /// <summary>
        /// the index of time column in the file.
        /// </summary>
        private int mTimeIndex;


        /// <summary>
        /// saves last datetime of updated coordinates to stay insync with file
        /// </summary>
        TimeSpan currLineTime = TimeSpan.MinValue;

        /// <summary>
        /// how many milli seconds to wait for applying line read to hand coordinates
        /// </summary>
        double mTimeBetweenFileLines = 0;

        /// <summary>
        /// stores next line to be read when time is right and update coordinates
        /// </summary>
        string[] mNextLineToUpdate;

        /// <summary>
        /// stores the datetime for the next update of coordinates
        /// </summary>
        DateTime readStartTime;

        /// <summary>
        /// List of lines after pre processing to delte very  close lines (time wise)
        /// </summary>
        public List<string[]> mCsvLines = new List<string[]>();

        /// <summary>
        /// List of time stamps coupled to a csv lines.
        /// </summary>
        List<TimeSpan> mCsvTimes = new List<TimeSpan>();

        /// <summary>
        /// the accumulated error to deduct from waiting time
        /// </summary>
        public double mErrorMsec;

        /// <summary>
        /// the index of the earliest ready line
        /// </summary>
        public int mCurrentLineIndex;
        #endregion

        #region Constructors
        
        /// <summary>
        /// constructs the CSV file reader
        /// </summary>
        /// <param name="file">the file to read from</param>
        public TimedCSVReader(CSVFile file, int timeColumnIndex, int minMsecDelta)
        {
            mFile = file;
            mCurrentLineIndex = 0;
            mErrorMsec = 0;
            string[] line;
            try
            {
                line = mFile.ReadLine();
            }
            catch (Exception)
            {
                UnityEngine.Debug.Log("can't read from file");
                throw;
            }
            
            mCsvTimes.Add(TimeSpan.Parse(line[timeColumnIndex]));            
            mCsvLines.Add(line);

            while (line != null)
            {
                line = mFile.ReadLine();
                if (line == null)
                {
                    break;
                }
                TimeSpan currentLineDT = TimeSpan.Parse(line[timeColumnIndex]);
                TimeSpan lastDT = mCsvTimes.Last();
                if ((currentLineDT - lastDT).TotalMilliseconds > minMsecDelta)
                {
                    mCsvTimes.Add(currentLineDT);
                    mCsvLines.Add(line);
                }
            }
           
            mTimeIndex = timeColumnIndex;
        }

        #endregion

        /// <summary>
        /// the functino reads a line from the CSVFile.
        /// if the file is finished or if it's yet the time to read the line null is returned.
        /// </summary>
        /// <returns>an array of csv values of a single line, 
        /// or null if file has finished or we need to wait more time for next read</returns>
        public string[] ReadLine()
        {
            if (mCurrentLineIndex >= mCsvLines.Count)
            {
                return null;
            }

            // set first date time
            if (currLineTime == TimeSpan.MinValue)
            {
                currLineTime = mCsvTimes[0];
                readStartTime = DateTime.Now;                               
            }
             
            string[] currentLine = null;
            double timeSinceReadStartMsec = (DateTime.Now - readStartTime).TotalMilliseconds;
            // check if we are redy for next line                         
            while (timeSinceReadStartMsec > currLineTime.TotalMilliseconds)
            {
                // if first read, actually read otherwise get next line the was previously read
                currentLine = (mNextLineToUpdate == null) ? mCsvLines[mCurrentLineIndex] : mNextLineToUpdate;
                // if end of file
                if (currentLine == null)
                {
                    return null;
                }

                // is end of file?
                if (++mCurrentLineIndex >= mCsvLines.Count)
                {
                    return null;
                }

                // get next line and next line time
                mErrorMsec = timeSinceReadStartMsec - currLineTime.TotalMilliseconds;
                Debug.Log("mErrorMsec = " + mErrorMsec);                
                mNextLineToUpdate = mCsvLines[mCurrentLineIndex];
                currLineTime = mCsvTimes[mCurrentLineIndex];
                //
                //mErrorMsec = Math.Min(mErrorMsec, mTimeBetweenFileLines);
                //UnityEngine.Debug.Log("Real time took : " + (DateTime.Now - mLastCoordinatesUpdated).TotalMilliseconds + " Msecs");
                //UnityEngine.Debug.Log("File time took : " + mTimeBetweenFileLines + " Msecs");
                //UnityEngine.Debug.Log("Error : " + mErrorMsec + " Msecs");                  
            }                
            
            return currentLine;
        }
    }
}
