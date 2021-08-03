using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed class ConfigurationManager
    {
        
        #region Data Members
        /// <summary>
        /// the instance of the singletone
        /// </summary>
        private static readonly ConfigurationManager sInstance = new ConfigurationManager();

        /// <summary>
        /// the configuratino file
        /// </summary>
        private FileRepository mConfigurationFile;

        /// <summary>
        /// the runtime configuration file,
        /// from which we will get path for actual experiment confgiruation (provides indirection of configuration)
        /// </summary>
        private FileRepository mRuntimeConfigurationFile;

        /// <summary>
        /// The configuration object we are currently working with
        /// </summary>
        private ExperimentConfiguration mConfigurationObject;

        #endregion

        #region Props


        /// <summary>
        /// static Singletne getter - first get will create the manager
        /// </summary>
        public static ConfigurationManager Instance
        {
            get
            {
                return sInstance;
            }
        }

        /// <summary>
        /// getter for configuration object
        /// </summary>
        public ExperimentConfiguration Configuration { get { return mConfigurationObject; } }

        #endregion

        #region Ctors

        // singletone implementation
        static ConfigurationManager()
        {
        }
        private ConfigurationManager()
        {
            mRuntimeConfigurationFile = new FileRepository();
            mRuntimeConfigurationFile.Connect(RuntimeConfiguration.RUNTIME_CONF_FILE_NAME);
            var runtimeConf = mRuntimeConfigurationFile.GetObject<RuntimeConfiguration>();
            if (!File.Exists(runtimeConf.PathToConfigurationFile))
            {
                UnityEngine.Debug.Log("Cannot find configuration.xml file in main directory! need to create one or move it to the right spot");
                return;
            }
            mConfigurationFile = new FileRepository();
            mConfigurationFile.Connect(runtimeConf.PathToConfigurationFile);

            mConfigurationObject = mConfigurationFile.GetObject<ExperimentConfiguration>();
        } 
        #endregion
    }
}
