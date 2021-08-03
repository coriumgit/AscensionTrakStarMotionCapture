using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTools;
using JasHandExperiment.Configuration;

namespace JasHandExperiment
{
    /// <summary>
    /// Singletine class that provides services for reading from congiguration 
    /// </summary>
    public sealed class ExperimentRuntime : UnityEngine.Object
    {
        
        #region Data Members
        /// <summary>
        /// the instance of the singletone
        /// </summary>
        private static readonly ExperimentRuntime sInstance = new ExperimentRuntime();
        
        /// <summary>
        /// The configuration object we are currently working with
        /// </summary>
        private ExperimentConfiguration mConfigurationObject;
        
        #endregion

        #region Props


        /// <summary>
        /// static Singletne getter - first get will create the manager
        /// </summary>
        public static ExperimentRuntime Instance
        {
            get
            {
                return sInstance;
            }
        }

		public uint TrialNumber { get; set; }
        

		public bool FirstRun { get; set; }

		public bool EndOfExperement { get; set; }
        #endregion

        #region Ctors

        // singletone implementation
        static ExperimentRuntime()
        {}

        private ExperimentRuntime()
        { 
			
            mConfigurationObject = ConfigurationManager.Instance.Configuration;
            TrialNumber = 1;
			FirstRun = false;
			EndOfExperement = false;
            CreatOutputFilesDirectories();
        }

        #endregion

        #region Functions

        /// <summary>
        /// The function creates both user presses and glove movements directories
        /// </summary>
        private void CreatOutputFilesDirectories()
        {
            // create output files directories
            // create glove movements directory for current participant if it doesn't exists
            if (!Directory.Exists(mConfigurationObject.OutputFilesConfiguration.GloveMovementLogPath))
            {
                Directory.CreateDirectory(mConfigurationObject.OutputFilesConfiguration.GloveMovementLogPath);
            }
            string currentSubjectGloveDir = mConfigurationObject.OutputFilesConfiguration.GloveMovementLogPath +
                mConfigurationObject.ParticipantConfiguration.Number;
            if (!Directory.Exists(currentSubjectGloveDir))
            {
                Directory.CreateDirectory(currentSubjectGloveDir);
            }

            // create user presses directory for current participant if it doesn't exists
            if (!Directory.Exists(mConfigurationObject.OutputFilesConfiguration.UserPressesLogPath))
            {
                Directory.CreateDirectory(mConfigurationObject.OutputFilesConfiguration.UserPressesLogPath);
            }
            string currentSubjectUserPresses = mConfigurationObject.OutputFilesConfiguration.UserPressesLogPath +
                mConfigurationObject.ParticipantConfiguration.Number;
            if (!Directory.Exists(currentSubjectUserPresses))
            {
                Directory.CreateDirectory(currentSubjectUserPresses);
            }
        }

        #endregion
    }
}
