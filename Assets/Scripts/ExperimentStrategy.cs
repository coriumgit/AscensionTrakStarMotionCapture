using CommonTools;
using JasHandExperiment.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static JasHandExperiment.ExperimentManager;
using System.IO;
using UnityEngine.UI;
namespace JasHandExperiment
{
    /// <summary>
    /// Abstract class for an experiment strategy
    /// </summary>
    public abstract class BaseExperimentStrategy
    {
        #region Data Members                                 

        private const int INIT_SAMPLES_NR = 40;

        protected HandSolverBase handSolver;

        private CalibrationPhase calibrationPhase;

        public bool IsCalibrating {
            get { return calibrationPhase != CalibrationPhase.NOT_CALIBRATING; }
        }

        private Dictionary<Tuple<FingerType, FingerSection>, Vector3> sensorsPosesInit;

        Dictionary<Tuple<FingerType, FingerSection>, Matrix4x4> sensorsRotsMatsInitTransposed;

        private List<FingerType> resolutionList;

        private ExperimentManager experimentManager;

        private Text experimenterMsgsDisplay;        

        /// <summary>
        /// device to read from the hands movement coordinats
        /// </summary>
        protected IHandMovementDevice mDevice;

        /// <summary>
        /// holds the controller of the hand to associate with fingers
        /// </summary>
        protected HandController mHandController;

        /// <summary>
        /// boolean indicating whether the strategy was initialized
        /// </summary>
        protected bool mIsInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// getter for experiment type
        /// </summary>
        public abstract ExperimentType Type { get; }

        #endregion

        #region Fucntions

        /// <summary>
        /// The fucntion initializaes the experiment  members
        /// </summary>
        /// <param name="handController">the hand controller to apply experiment process to</param>
        public void Init(HandController handController)
        {
            if (mIsInitialized)            
                return;
            
            mIsInitialized = true;            
            mHandController = handController;
            mDevice = HandMovemventDeviceFactory.GetOrCreate(Type);
            // start reading from device
            mDevice.Open();

            experimentManager = GameObject.Find("ExperimentManager").GetComponent<ExperimentManager>();
            experimenterMsgsDisplay = GameObject.Find("experimenterMsgs").GetComponent<Text>();
            
            sensorsPosesInit = new Dictionary<Tuple<FingerType, FingerSection>, Vector3>();
            sensorsRotsMatsInitTransposed = new Dictionary<Tuple<FingerType, FingerSection>, Matrix4x4>();
            foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor)))
            {
                Tuple<FingerType, FingerSection> fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor));
                sensorsPosesInit.Add(fingerTypeSectionDuo, new Vector3());
                sensorsRotsMatsInitTransposed.Add(fingerTypeSectionDuo, new Matrix4x4());                
            }
            initSensorsTransformVars();

            List<FingerType> resolutionList = new List<FingerType>();
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))
                resolutionList.Add(fingerType);

            Transform handJoint = CommonUtilities.FindObjectWithName(handController.GetComponent<Transform>(), "hand_joint");
            if (Type == ExperimentType.Active) {
                handSolver = new HandSolverBySensors(handJoint, sensorsPosesInit, sensorsRotsMatsInitTransposed, resolutionList);
                calibrationPhase = CalibrationPhase.ALIGNING_REAL_AND_VIRTUAL_HANDS;
            }
            else if (Type == ExperimentType.PassiveWatchingReplay)
            {
                // REPLAY RELATEDs
                handSolver = new HandSolverByTransformsData(handJoint, Path.ChangeExtension(ConfigurationManager.Instance.Configuration.ReplayFilePath, ".dat"));
                calibrationPhase = CalibrationPhase.NOT_CALIBRATING;
            }

            InnerInit();                                   
        }

        /// <summary>
        /// inner init for each specific initialization needs.
        /// being called before opening the device!
        /// </summary>
        public virtual void InnerInit()
        {
            // not obligatory
        }
        
        /// <summary>
        /// The function is responsible for hand movement. can be overwriten
        /// </summary>
        public virtual void MoveHand()
        {            
            Vector3 wristLookDirection = Vector3.forward;
            HandCoordinatesData handCoordinatesData = mDevice.GetHandData() as HandCoordinatesData;
            if (handCoordinatesData[GuidanceSensor.INDEX_PROX][0] == 0.0f)
                return;
            
            Dictionary<Tuple<FingerType,FingerSection>, Vector3> sensorsTranslations = new Dictionary<Tuple<FingerType,FingerSection>, Vector3>();
            Dictionary<Tuple<FingerType,FingerSection>, Matrix4x4> sensorsRotsMats = new Dictionary<Tuple<FingerType,FingerSection>, Matrix4x4>();
            foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor))) {
                Tuple<FingerType,FingerSection> fingerTypeSectionDuo = new Tuple<FingerType,FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor));
                sensorsTranslations.Add(fingerTypeSectionDuo, new Vector3(handCoordinatesData[sensor][0], handCoordinatesData[sensor][1], -handCoordinatesData[sensor][2]) - sensorsPosesInit[fingerTypeSectionDuo]);               
                sensorsRotsMats.Add(fingerTypeSectionDuo, compSensor2globalRotMat(handCoordinatesData[sensor][3], handCoordinatesData[sensor][4], handCoordinatesData[sensor][5])*sensorsRotsMatsInitTransposed[fingerTypeSectionDuo]);
            }
            
            if (Type == ExperimentType.Active)
            {
                ((HandSolverBySensors)handSolver).updateSensorsVals(sensorsTranslations, sensorsRotsMats);
                if (calibrationPhase != CalibrationPhase.NOT_CALIBRATING && Input.GetKeyDown(KeyCode.C))
                {
                    experimenterMsgsDisplay.text = "";
                    recalibrate();
                }

                if (calibrationPhase == CalibrationPhase.ALIGNING_REAL_AND_VIRTUAL_HANDS)
                {
                    //TODO: update experimenterMsgsDisplay to display all 16 biases at the same time                    
                    //experimenterMsgsDisplay.text = "";
                    //float[] biases = new float[4];                   
                    ((HandSolverBySensors)handSolver).simulateJointsSensors();
                    //for (int biasIdx = 0; biasIdx < 4; biasIdx++)
                    //{
                    //    experimenterMsgsDisplay.text += biases[biasIdx].ToString("0.0");
                    //    if (biasIdx < 3)
                    //        experimenterMsgsDisplay.text += ", ";
                    //}
                    //experimenterMsgsDisplay.text += '\n';                                      
                    
                    if (Input.GetKeyDown(KeyCode.R))
                    {                        
                        ((HandSolverBySensors)handSolver).alignJointsSensorsToJointsAvatars();
                        initSensorsTransformVars();
                    }
                    else if (Input.GetKeyDown(KeyCode.Return))
                    {                        
                        ((HandSolverBySensors)handSolver).disableJointsSensorsAndAvatars();
                        initSensorsTransformVars();
                        calibrationPhase = CalibrationPhase.ALIGNING_REAL_AND_VIRTUAL_RESPONSE_BOXES;                                               
                    }

                    return;
                }
                else if (calibrationPhase == CalibrationPhase.ALIGNING_REAL_AND_VIRTUAL_RESPONSE_BOXES && Input.GetKeyDown(KeyCode.Return))
                {                                        
                    int btnIdxUnderIndexFinger = 3;
                    int btnIdxUnderLittleFinger = 0;
                    if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == HandType.Right)
                    {
                        btnIdxUnderIndexFinger = 0;
                        btnIdxUnderLittleFinger = 3;
                    }

                    Transform responseBoxTransform = GameObject.FindGameObjectWithTag("ResponseBox").transform;                    
                    Vector3 indexTipPos = handSolver.getFingerTipPos(FingerType.INDEX);
                    Vector3 littleTipPos = handSolver.getFingerTipPos(FingerType.LITTLE);                    
                    responseBoxTransform.localScale *= 1.05f * Vector3.Distance(indexTipPos, littleTipPos) / Vector3.Distance(responseBoxTransform.GetChild(btnIdxUnderIndexFinger).position, responseBoxTransform.GetChild(btnIdxUnderLittleFinger).position);
                    indexTipPos = handSolver.getFingerTipPos(FingerType.INDEX);
                    responseBoxTransform.Translate(indexTipPos - responseBoxTransform.GetChild(btnIdxUnderIndexFinger).position, Space.World);
                    responseBoxTransform.Translate(0.0f, -(responseBoxTransform.GetChild(btnIdxUnderIndexFinger).localScale.y + 0.5f*responseBoxTransform.localScale.y), 0.0f);
                    indexTipPos = handSolver.getFingerTipPos(FingerType.INDEX);
                    littleTipPos = handSolver.getFingerTipPos(FingerType.LITTLE);
                    Vector3 meanResponseBoxDirection = (littleTipPos - indexTipPos).normalized; //(handSolver.getFingerTipPos(FingerType.MIDDLE) - indexTipPos).normalized + (handSolver.getFingerTipPos(FingerType.RING) - indexTipPos).normalized + 
                    //responseBoxTransform.Rotate(Quaternion.FromToRotation(responseBoxTransform.GetChild(btnIdxUnderLittleFinger).position - responseBoxTransform.GetChild(btnIdxUnderIndexFinger).position, (littleTipPos - indexTipPos)).eulerAngles);
                    //responseBoxTransform.Rotate(Quaternion.FromToRotation(new Vector3(responseBoxTransform.GetChild(btnIdxUnderLittleFinger).position.x - responseBoxTransform.GetChild(btnIdxUnderIndexFinger).position.x, 0.0f, responseBoxTransform.GetChild(btnIdxUnderLittleFinger).position.z - responseBoxTransform.GetChild(btnIdxUnderIndexFinger).position.z), new Vector3(meanResponseBoxDirection.x, 0.0f, meanResponseBoxDirection.z)).eulerAngles);                                        
                    calibrationPhase = CalibrationPhase.WAITING_ON_END_CALIBRATION_BTN;
                    experimenterMsgsDisplay.text = "Press enter to accept calibration";
                }
                else if (calibrationPhase == CalibrationPhase.WAITING_ON_END_CALIBRATION_BTN && Input.GetKeyDown(KeyCode.Return))
                {                    
                    experimentManager.resumeTrial();
                    calibrationPhase = CalibrationPhase.NOT_CALIBRATING;                    
                }                                
            }                        
            else if (Type == ExperimentType.PassiveWatchingReplay)
                experimentManager.resumeTrial();

            handSolver.resolve();
        }

        public void recalibrate()
        {
            ((HandSolverBySensors)handSolver).revertHandToBindPose();
            initSensorsTransformVars();
            calibrationPhase = CalibrationPhase.ALIGNING_REAL_AND_VIRTUAL_HANDS;
        }

        public void startLogging()
        {
            if (Type == ExperimentType.Active)
            {
                ((GlovesDevice)mDevice).startLogging();
                ((HandSolverBySensors)handSolver).startRecording();
            }
        }

        public void pauseLogging()
        {
            if (Type == ExperimentType.Active)
            {
                ((GlovesDevice)mDevice).pauseLogging();
                ((HandSolverBySensors)handSolver).pauseRecording();
            }
        }

        public void resumeLogging()
        {
            if (Type == ExperimentType.Active)
            {
                ((GlovesDevice)mDevice).resumeLogging();
                ((HandSolverBySensors)handSolver).resumeRecording();
            }
        }

        public void saveLogging()
        {
            // REPLAY RELATED
            if (Type == ExperimentType.Active)
                ((HandSolverBySensors)handSolver).saveTransformsDataToFile(Path.ChangeExtension(Environment.CurrentDirectory + CommonUtilities.GetParticipantAvatarStateFileName(ConfigurationManager.Instance.Configuration.OutputFilesConfiguration.GloveMovementLogPath), ".dat"));
        }

        private void initSensorsTransformVars()
        {
            foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor)))
            {
                Tuple<FingerType, FingerSection> fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor));
                sensorsPosesInit[fingerTypeSectionDuo] = new Vector3();
                sensorsRotsMatsInitTransposed[fingerTypeSectionDuo] = new Matrix4x4();                
            }

            Dictionary<Tuple<FingerType, FingerSection>, Vector3> sensorsRotsEuler = new Dictionary<Tuple<FingerType, FingerSection>, Vector3>();
            foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor)))                            
                sensorsRotsEuler.Add(new Tuple<FingerType, FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor)), new Vector3());            

            for (int initSampleIdx = 0; initSampleIdx < INIT_SAMPLES_NR; initSampleIdx++)
            {
                HandCoordinatesData initCoordsData = mDevice.GetHandData() as HandCoordinatesData;
                foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor)))
                {
                    Tuple<FingerType, FingerSection> fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor));
                    sensorsPosesInit[fingerTypeSectionDuo] += new Vector3(initCoordsData[sensor][0], initCoordsData[sensor][1], -initCoordsData[sensor][2]);
                    sensorsRotsEuler[fingerTypeSectionDuo] += new Vector3(initCoordsData[sensor][3], initCoordsData[sensor][4], initCoordsData[sensor][5]);
                }
            }
            foreach (GuidanceSensor sensor in Enum.GetValues(typeof(GuidanceSensor)))
            {
                Tuple<FingerType, FingerSection> fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(GuidenceSensorExtension.toFinger(sensor), GuidenceSensorExtension.toSection(sensor));
                sensorsPosesInit[fingerTypeSectionDuo] /= INIT_SAMPLES_NR;
                Vector3 sensorRotsEulerMean = sensorsRotsEuler[fingerTypeSectionDuo] / INIT_SAMPLES_NR;
                sensorsRotsMatsInitTransposed[fingerTypeSectionDuo] = compSensor2globalRotMat(sensorRotsEulerMean.x, sensorRotsEulerMean.y, sensorRotsEulerMean.z).transpose;
            }

        }

        private Vector3 compLookAtDirection(Matrix4x4 sensor2globalRotMat)
        {
            Vector3 lookAtDirectionInversedZ = sensor2globalRotMat.MultiplyVector(Vector3.right);

            return new Vector3(lookAtDirectionInversedZ.x, lookAtDirectionInversedZ.y, -lookAtDirectionInversedZ.z);
        }

        private Vector3 compUpDirection(Matrix4x4 sensor2globalRotMat)
        {
            Vector3 upDirectionInversedZ = sensor2globalRotMat.MultiplyVector(Vector3.down);

            return new Vector3(upDirectionInversedZ.x, upDirectionInversedZ.y, -upDirectionInversedZ.z);
        }

        private static Matrix4x4 compSensor2globalRotMat(float a, float e, float r)
        {
            a *= Mathf.Deg2Rad; e *= Mathf.Deg2Rad; r *= Mathf.Deg2Rad;
            // according to page 132 in trakSTAR manual - the following computed matrix transforms a vector 
            // from global coordinates to sensor coordinates.
            // the axis the sensor is aligned to in it's coordinate system is X =>
            // the direction the sensor is pointing aquels to this matrix transposed multiplied by (1,0,0)
            float m11 = Mathf.Cos(e) * Mathf.Cos(a);
            float m12 = Mathf.Cos(e) * Mathf.Sin(a);
            float m13 = -Mathf.Sin(e);
            float m21 = -Mathf.Cos(r) * Mathf.Sin(a) + Mathf.Sin(r) * Mathf.Sin(e) * Mathf.Cos(a);
            float m22 = Mathf.Cos(r) * Mathf.Cos(a) + Mathf.Sin(r) * Mathf.Sin(e) * Mathf.Sin(a);
            float m23 = Mathf.Sin(r) * Mathf.Cos(e);
            float m31 = Mathf.Sin(r) * Mathf.Sin(a) + Mathf.Cos(r) * Mathf.Sin(e) * Mathf.Cos(a);
            float m32 = -Mathf.Sin(r) * Mathf.Cos(a) + Mathf.Cos(r) * Mathf.Sin(e) * Mathf.Sin(a);
            float m33 = Mathf.Cos(r) * Mathf.Cos(e);
            // Reminder: Matrix4x4 ctr takes the columns vectors. we feed it the rows as columns to construct the transposed matrix.
            return new Matrix4x4(new Vector4(m11, m12, m13, 0.0f),
                                 new Vector4(m21, m22, m23, 0.0f),
                                 new Vector4(m31, m32, m33, 0.0f),
                                 new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        }

        /// <summary>
        /// the function closes the experiment. can be overriten
        /// </summary>
        public virtual void Close()
        {
            if (mDevice == null)
            {
                return;
            }
            mIsInitialized = false;          
            mDevice.Close();            
        } 
        #endregion
    }

    /// <summary>
    /// class for Active gloves experiments
    /// </summary>
    public class ActiveExperimentStrategy : BaseExperimentStrategy
    {
        private GlovesDevice mGloveDevice;

        public override ExperimentType Type
        {
            get
            {
                return ExperimentType.Active;
            }
        }

        public override void InnerInit()
        {
            base.InnerInit();
            mGloveDevice = mDevice as GlovesDevice;
        }

        public override void MoveHand()
        {
            if (CalibrationManager.Mode == HandPlayMode.Calibration)
            {
                CalibrationManager.HandCalibrationUserInput();
            }
            base.MoveHand();
        }        
    }

    /// <summary>
    /// class for passive replay from old active experiemnt
    /// </summary>
    public class PassiveReplayExperimentStrategy : BaseExperimentStrategy
    {
        public override ExperimentType Type
        {
            get
            {
                return ExperimentType.PassiveWatchingReplay;
            }
        }
    }

    /// <summary>
    /// class for simulation, reading from a file a sqeuence of simulated key presses and applying relevant animations
    /// </summary>
    public class PassiveSimulationExperimentStrategy : BaseExperimentStrategy
    {
        #region Data Members
        // manages animation of the hand for simulation experiment type
        private AnimationManager mAnimationManager;

        string mLastDTUpdated;
        #endregion

        #region Properties
        public override ExperimentType Type
        {
            get
            {
                return ExperimentType.PassiveSimulation;
            }
        }

        #endregion

        #region Functions

        /// <summary>
        /// The functions applies init for simulation
        /// </summary>
        public override void InnerInit()
        {
            mLastDTUpdated = DateTime.MinValue.ToLongTimeString();
            InitHandAnimator();
        }

        /// <summary>
        /// the fucntino applies anumation manager reactins accordign to device's output
        /// </summary>
        public override void MoveHand()
        {
            var keyPressedData = mDevice.GetHandData() as KeyPressedData;
            if (keyPressedData == null || string.IsNullOrEmpty(keyPressedData.TimeStamp))
            {
                return;
            }
            if (keyPressedData.TimeStamp.Equals(mLastDTUpdated))
            {
                mAnimationManager.SetFalseToAllProps();
                return;
            }
            mLastDTUpdated = keyPressedData.TimeStamp;

            UnityEngine.Debug.Log("Current animation to play : " + keyPressedData.KeyPressed);

            mAnimationManager.RespondExclusivleyToTrigger(keyPressedData.KeyPressed);
        }

        /// <summary>
        /// the functino initialzied the animation manager to be later used
        /// </summary>
        private void InitHandAnimator()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map.Add(CommonConstants.INDEX_KEY_PRESS_STRING, CommonConstants.INDEX_KEY_PRESS_PARAM);
            map.Add(CommonConstants.MIDDLE_KEY_PRESS_STRING, CommonConstants.MIDDLE_KEY_PRESS_PARAM);
            map.Add(CommonConstants.RING_KEY_PRESS_STRING, CommonConstants.RING_KEY_PRESS_PARAM);
            map.Add(CommonConstants.PINKY_KEY_PRESS_STRING, CommonConstants.PINKY_KEY_PRESS_PARAM);
            mAnimationManager = new AnimationManager(mHandController, map);
        } 
        #endregion

    }
}
