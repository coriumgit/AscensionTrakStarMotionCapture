using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CommonTools
{
    /// <summary>
    /// Interface for a comma seperated file.
    /// Contains functions of write, read close and init.
    /// </summary>
    public interface ICSVFile
    {
        /// <summary>
        /// The function writes given line according to the format given in Init function.
        /// </summary>
        /// <param name="line">the lien to write. spaces will be replaced by seperator of the CSV file given in Init function</param>
        /// <returns>boolean indicating success of operation</returns>
        bool WriteLine(string line);

        /// <summary>
        /// The function writes given line (in seperate values array) according to the format given in Init function.
        /// </summary>
        /// <param name="valuePerColumn">string array, each element is a different value of a line in the CSV file</param>
        /// <returns>boolean indicating success of operation</returns>
        bool WriteLine(string[] valuePerColumn);

        /// <summary>
        /// The function writes given lines (given in IEnumerable of arrays of seperate values array of strings),
        /// according to the format given in Init function.
        /// </summary>
        /// <param name="valuePerColumnEnumer">enumerator of string arrays, each element in the array is a different value of a line in the CSV file</param>
        /// <returns>boolean indicating success of operation</returns>
        bool WriteLines(IEnumerable<string[]> valuePerColumnEnumer);

        /// <summary>
        /// The function writes given lines according to the format given in Init function.
        /// </summary>
        /// <param name="valuePerColumnEnumer">enumerator of strings, each string is a line in the CSV file</param>
        /// <returns>boolean indicating success of operation</returns>
        bool WriteLines(IEnumerable<string> lines);

        /// <summary>
        /// The function initializes the CSV file according to parameters. the function MUST be called after allocating an instance of CSVFile.
        /// </summary>
        /// <param name="fileName">the name of the CSV file (path to it)</param>
        /// <param name="mode">the mode of the file - open/create for read/write</param>
        /// <param name="seperator">the seperator between items of the same line in the csv file</param>
        /// <param name="csvColumns">the columns in the top of the csv file</param>
        /// <param name="settings">a structure that supply settings for how the CSV fiel will be written and how to read from it</param>
        void Init(string fileName, FileMode mode, string seperator = ",", IEnumerable<string> csvColumns = null, IBaseCSVRWSettings settings = null);

        /// <summary>
        /// function that MUST be called after you finished using the file.
        /// the function flushes all pending lines to the file.
        /// </summary>
        void Close();

        /// <summary>
        /// The function reads lines from the file and returns the lines.
        /// settings structure given in Init function will dictate the configuration of reading process.
        /// </summary>
        /// <returns>the function returns an ienumerable of lines read from file, each item is a string array of all of the values of the line</returns>
        IEnumerable<string[]> ReadLines();

        /// <summary>
        /// The function returns a single line from the file.
        /// </summary>
        /// <returns>the function returns a string array, each element in the array is a value of the line read from the CSV file.</returns>
        string[] ReadLine();
    }

    /// <summary>
    /// Interface for a settings structure that will be handles by a ICSVFile implementation
    /// </summary>
    public interface IBaseCSVRWSettings
    {

    }


    /// <summary>IBaseCSVRWSettings
    /// implements IBaseCSVRWSettings to be used by a ICSVFile.
    /// provides configuration for read/write and batching.
    /// </summary>
    [Serializable]
    public class BatchCSVRWSettings : IBaseCSVRWSettings
    {
        #region Constants
        public const uint DEFAULT_READ_BATCH_SIZE = 1;
        public const uint DEFAULT_WRITE_BATCH_SIZE = 1;

        public const uint DEFAULT_WRITE_DELAY_MSEC = 0;
        public const uint DEFAULT_READ_DELAY_MSEC = 0;
        #endregion

        #region Propreties
        /// <summary>
        /// number of lines to read in batch mode
        /// </summary>
        public uint ReadBatchSize { get; set; }

        /// <summary>
        /// number of lines to write in batch mode
        /// </summary>
        public uint WriteBatchSize { get; set; }

        /// <summary>
        /// number of msecs to wait between reading batches of lines to file
        /// </summary>
        public uint ReadBatchDelayMsec { get; set; }

        /// <summary>
        /// number of msecs to wait between writing batches of lines to file
        /// </summary>
        public uint WriteBatchDelayMsec { get; set; }

        #endregion

        #region Ctors
        public BatchCSVRWSettings()
        {
            ReadBatchSize = DEFAULT_READ_BATCH_SIZE;
            WriteBatchSize = DEFAULT_WRITE_BATCH_SIZE;

            ReadBatchDelayMsec = DEFAULT_READ_DELAY_MSEC;
            WriteBatchDelayMsec = DEFAULT_WRITE_DELAY_MSEC;
        } 
        #endregion
    }

    /// <summary>
    /// The class is an implemetation of ICSVFile interface.
    /// provides functionality of batching to a CSV file.
    /// enables async/sync flacours of read/write according to settings structure given in Init function.   
    /// </summary>
    public class CSVFile : ICSVFile
    {
        #region Data Members

        /// <summary>
        /// The actual file stream
        /// </summary>
        protected FileStream mFile;

        /// <summary>
        /// the colums on the top of the file to be written (in a new opened files) or ignore if we are reding lines from the file.
        /// </summary>
        protected IEnumerable<string> mColumns;
        
        /// <summary>
        /// te seperator to be used in the file
        /// </summary>
        protected string mSeperator;
        
        /// <summary>
        /// writer to write lines with
        /// </summary>
        protected StreamWriter mWriter;

        /// <summary>
        /// reader to read lines with
        /// </summary>
        protected StreamReader mReader;

        /// <summary>
        /// holds the settings given in Init function.
        /// enables control of Async/Sync functionality of Read/Write.
        /// </summary>
        protected IBaseCSVRWSettings mSettings;
        
        /// <summary>
        /// holds read thread, if sync settigns applied it will stay null
        /// </summary>
        private Thread mReadThread;

        /// <summary>
        /// holds write thread, if sync settigns applied it will stay null
        /// </summary>
        private Thread mWriteThread;

        /// <summary>
        /// boolean that keeps write/read thread alive
        /// </summary>
        private bool mIsAlive = true;

        /// <summary>
        /// AutoResetEvent to signal write events
        /// used in write thread
        /// </summary>
        private AutoResetEvent mWThreadSleepEvent;

        /// <summary>
        /// AutoResetEvent to signal read events.
        /// used in read thread
        /// </summary>
        private AutoResetEvent mRThreadSleepEvent;

        /// <summary>
        /// Queue of ending lines to be written in write thread
        /// </summary>
        private Queue<string[]> mRemainingBatchUnSeperated;

        /// <summary>
        /// lock object for read/write threads
        /// </summary>
        private object mQueueLock;
        #endregion

        /// <summary>
        /// Event to signal out when lines are ready are were read from file
        /// </summary>
        public event Action<IEnumerable<string[]>> OnLinesReady;

        /// <summary>
        /// property made for convinience, we are using btach settings in batch csv..
        /// </summary>
        public BatchCSVRWSettings Settings
        {
            get
            {
                return mSettings as BatchCSVRWSettings;
            }
        }

        /// <summary>
        /// Refer to the interface Init function for documentation. 
        /// </summary>
        void ICSVFile.Init(string fileName, FileMode mode, string seperator, IEnumerable<string> csvColumns, IBaseCSVRWSettings settings)
        {
            Init(fileName, mode,seperator, csvColumns, settings);
        }

        /// <summary>
        /// Refer to the interface Init function for documentation. 
        /// </summary>
        void Init(string fileName, FileMode mode, string seperator = ",", IEnumerable<string> csvColumns = null, IBaseCSVRWSettings settings = null)
        {
            FileAccess acc = mode == FileMode.Open ? FileAccess.Read : FileAccess.ReadWrite;

            try
            {
                var fs = new FileStream(fileName, mode, FileAccess.ReadWrite);
                Init(fs, seperator, csvColumns, settings);
            }
            catch (FileNotFoundException notFoundEx)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("The file ");
                sb.Append(fileName + " doesn't exist !! ");
                sb.AppendLine("exception : " + notFoundEx.Message);
                Debug.Log(sb.ToString());
                throw notFoundEx;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("exception : " + ex.Message);
                Debug.Log(sb.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Refer to the interface Init function for documentation. 
        /// </summary>
        public void Init(FileStream fs, char seperator = ',', IEnumerable<string> csvColumns = null, IBaseCSVRWSettings settings = null) 
        {
            Init(fs, seperator.ToString(), csvColumns, settings);
        }

        /// <summary>
        /// Refer to the interface Init function for documentation. 
        /// </summary>
        public void Init(string fileName, FileMode mode, char seperator = ',', IEnumerable<string> csvColumns = null, IBaseCSVRWSettings settings = null)
        {
            Init(fileName, mode, seperator.ToString(), csvColumns, settings);
        }

        /// <summary>
        /// Refer to the interface Init function for documentation. 
        /// </summary>
        public void Init(FileStream fs, string seperator = ",", IEnumerable<string> csvColumns = null, IBaseCSVRWSettings settings = null)
        {
            mFile = fs;
            mRemainingBatchUnSeperated = new Queue<string[]>();
            mWThreadSleepEvent = new AutoResetEvent(false);
            mRThreadSleepEvent = new AutoResetEvent(false);
            mQueueLock = new object();
            mColumns = csvColumns;
            mSeperator = seperator;
            mWriter = new StreamWriter(mFile);

            // auto flush to write all contents straight to file. we don'w want to loose data
            mWriter.AutoFlush = true;
            mReader = new StreamReader(mFile);
            mReader.BaseStream.Position = 0;
            mSettings = settings;
            if (mSettings == null)
            {
                // no settings given? use default one
                mSettings = new BatchCSVRWSettings();
            }
            // file has columns?
            if (csvColumns!= null)
            {
                // a new empty file? write a header of the columns 
                if (mFile.Length == 0)
                {
                    AppendLineToFile(csvColumns.ToArray());
                }
                else if (csvColumns != null)// old file? read the hedaer so users will only get lines of the file
                {
                    // read columns line
                    try
                    {
                        mReader.ReadLine();
                    }
                    catch (Exception)
                    {
                        Debug.Log("can't read from file : " + mFile.Name);
                    }
                }
            }
            
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public string[] ReadLine()
        {
            // sync read
            string line = null;

            try
            {
                line = mReader.ReadLine();
            }
            catch (Exception)
            {
                Debug.Log("can't read from file : " + mFile.Name);
            }
            if (line == null)
            {
                return null;
            }
            return line.Split(new string[] { mSeperator }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public IEnumerable<string[]> ReadLines()
        {
            IList<string[]> csvLines = new List<string[]>();

            // sync read
            if (Settings.ReadBatchDelayMsec == 0)
            {
                while (!mReader.EndOfStream)
                {
                    for (int i = 0; i < Settings.ReadBatchSize; i++)
                    {
                        csvLines.Add(ReadLine());
                    }
                }
            }
            else
            {
                // async read
                if (mReadThread == null)
                {
                    mReadThread = new Thread(new ThreadStart(ReaderThreadFunc));
                    mReadThread.Start();
                }
            }


            return csvLines;
        }

        /// <summary>
        /// The function of the reader thread.
        /// waits for write time to read and then reads a batch.
        /// when lines are ready, OnLinesReady event will be fired with relevant lines. 
        /// </summary>
        private void ReaderThreadFunc()
        {
            IList<string[]> csvLines = new List<string[]>();

            // while we are running
            while (mIsAlive && !mReader.EndOfStream)
            {
                // clear old lines
                csvLines.Clear();

                // read btach amount of lines
                for (int i = 0; i < Settings.ReadBatchSize; i++)
                {
                    string[] line = ReadLine();
                    if (line == null)
                    {
                        // end of file? break
                        break;
                    }
                    csvLines.Add(line);
                }

                if (csvLines.Count != 0 && OnLinesReady != null)
                {
                    // lines ready, fire event
                    OnLinesReady(csvLines);
                }

                // wait one on the ResetEvent accordign to settings.
                mRThreadSleepEvent.WaitOne((int)Settings.ReadBatchDelayMsec);
            }
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public bool WriteLine(string line)
        {
            if (mFile == null)
            {
                return false;
            }

            // sync write
            if (Settings.WriteBatchDelayMsec == 0)
            {
                AppendLineToFile(SeperateLine(line));
            }
            else
            {
                // async write
                lock (mQueueLock)
                {
                    // add pemnding line to queue
                    mRemainingBatchUnSeperated.Enqueue(SeperateLine(line));
                }
                // is thread running? create and run it pls
                CreateWriterThreadIfNeeded();
            }

            return true;
        }
        
        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public bool WriteLine(float[] valuePerColumn)
        {
            return WriteLine(StringUtilities.FloatToStringArray(valuePerColumn));
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public bool WriteLine(string[] valuePerColumn)
        {
            if (mFile == null)
            {
                return false;
            }

            // sync write
            if (Settings.WriteBatchDelayMsec == 0)
            {
                AppendLineToFile(valuePerColumn);
            }
            else
            {
                // async write
                lock (mQueueLock)
                {
                    // add line to pending lines
                    mRemainingBatchUnSeperated.Enqueue(valuePerColumn); 
                }
                // is thread running? create and run it
                CreateWriterThreadIfNeeded();
            }

            return true;
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public bool WriteLines(IEnumerable<string[]> valuePerColumnEnumer)
        {
            if (mFile == null)
            {
                return false;
            }
            // sync write
            if (Settings.WriteBatchDelayMsec == 0)
            {
                AppendLinesToFile(valuePerColumnEnumer);
            }
            else
            {
                // async write
                lock (mQueueLock)
                {
                    // add to queue pending line
                    foreach (var line in valuePerColumnEnumer)
                    {
                        mRemainingBatchUnSeperated.Enqueue(line);
                    } 
                }
                // if thread is not running, create and run it
                CreateWriterThreadIfNeeded();
            }
            return true;
        }

        /// <summary>
        /// refer to the interface for documentation
        /// </summary>
        public bool WriteLines(IEnumerable<string> lines)
        {
            if (mFile == null)
            {
                return false;
            }
            // sync write
            if (Settings.WriteBatchDelayMsec == 0)
            {
                List<string[]> seperatedLines = new List<string[]>();
                foreach (var line in lines)
                {
                    seperatedLines.Add(SeperateLine(line));
                }
                AppendLinesToFile(seperatedLines);
            }
            else
            {
                // async write, add to the queue, the lines are pending
                lock (mQueueLock)
                {
                    foreach (var line in lines)
                    {
                        mRemainingBatchUnSeperated.Enqueue(SeperateLine(line));
                    } 
                }
                // is the thread running? if not create it
                CreateWriterThreadIfNeeded();
            }
            return true;
        }

        /// <summary>
        /// Helper function, ensured writer thread will only be created once in given time.
        /// the fucntino also starts the thread
        /// </summary>
        private void CreateWriterThreadIfNeeded()
        {
            if (mWriteThread == null)
            {
                mWriteThread = new Thread(new ThreadStart(WriterThreadFunc));
                mWriteThread.Start();
            }
        }

        /// <summary>
        /// The function of writer thread.
        /// configured by settings structure given in Init function of the class.
        /// writes lines stored in the queue of the class when it is signaled to by the AutoResetEvent.
        /// </summary>
        private void WriterThreadFunc()
        {
            List<string[]> currentBatch = new List<string[]>();
            // whiel running
            while (mIsAlive)
            {
                // get batch of pending lines
                lock (mQueueLock)
                {
                    if (mRemainingBatchUnSeperated.Any())
                    {
                        int currentBatchSize = (int)Math.Min(mRemainingBatchUnSeperated.Count, Settings.WriteBatchSize);
                        for (int i = 0; i < currentBatchSize; i++)
                        {
                            string[] line = mRemainingBatchUnSeperated.Dequeue();
                            currentBatch.Add(line);
                        }
                        // actually add the lines to the file
                        AppendLinesToFile(currentBatch);
                    }
                }
                // old batch, clear it out
                currentBatch.Clear();
                // wait for a signal to write again
                mWThreadSleepEvent.WaitOne((int)Settings.WriteBatchDelayMsec);
            }

            // flush final lines in a single batch
            lock (mQueueLock)
            {
                if (mRemainingBatchUnSeperated.Any())
                {
                    while (mRemainingBatchUnSeperated.Any())
                    {
                        string[] line = mRemainingBatchUnSeperated.Dequeue();
                        currentBatch.Add(line);
                    }

                    AppendLinesToFile(currentBatch);
                }
            }
        }

        public virtual void Close()
        {
            // stop threads
            mIsAlive = false;
            if (mReadThread != null)
            {
                mRThreadSleepEvent.Set();
                mReadThread.Join();
            }
            if (mWriteThread != null)
            {
                mWThreadSleepEvent.Set();
                mWriteThread.Join();
            }
            // close the file
            mFile.Close();
        }

        /// <summary>
        /// helper function the seprated a strign line to elements,
        /// according to the seperator given in settings structure given in Init function.
        /// </summary>
        /// <param name="line">line to be split to elements</param>
        /// <returns>array of values the will be a line in a CSV file</returns>
        protected string[] SeperateLine(string line)
        {
            return line.Split(new string[] { mSeperator }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// The function appends all the given lines to the CSV file
        /// </summary>
        /// <param name="valuePerColumnEnumer"></param>
        protected void AppendLinesToFile(IEnumerable<string[]> valuePerColumnEnumer)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var line in valuePerColumnEnumer)
            {
                for (int i=0; i< line.Length; i++)
                {
                    sb.Append(line[i]);
                    if (i != line.Length -1)
                    {
                        sb.Append(mSeperator);
                    }
                }
                sb.AppendLine();
            }

            try
            {
                mWriter.Write(sb.ToString());
            }
            catch (Exception)
            {
                Debug.Log("can't write to file : " + mFile.Name);
            }
        }

        /// <summary>
        /// Appends a single line to the file
        /// </summary>
        /// <param name="line">the line to append to the CSV file</param>
        protected void AppendLineToFile(string[] line)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                sb.Append(line[i]);
                if (i != line.Length -1)
                {
                    sb.Append(mSeperator);
                }
            }
            try
            {
                mWriter.WriteLine(sb.ToString());
            }
            catch (Exception)
            {
                Debug.Log("can't write to file : " + mFile.Name);
                throw;
            }
        }

    }
}
