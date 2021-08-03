using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

// Reminder: Bone indexing starts from the hand bones
//           Joint indexing starts from the knuckles
//           Thus joint index corresponds to the bone of which it is the tip
namespace JasHandExperiment
{    
    public class HandSolverBySensors : HandSolverBase
    {
        protected class JointData
        {
            public Transform tippedBoneTransform { get; internal set; }
            public Vector3 pos { get; set; }
            public Vector3 posCache { get; set; }
            public float tippedBoneLen { get; internal set; }
            public readonly Vector3 posInit;
            private Vector3 transformPosInit;
            private Quaternion transformRotInit;
            public JointData(Transform t, Vector3 p, float l)
            {
                tippedBoneTransform = t;
                transformPosInit = t.position;
                transformRotInit = t.rotation;
                posInit = posCache = pos = p;
                tippedBoneLen = l;
            }
            public void revertToInit()
            {
                tippedBoneTransform.position = transformPosInit;
                tippedBoneTransform.rotation = transformRotInit;
                posCache = pos = posInit;
            }
        }

        private DateTime recordingStartTime;

        private readonly float[][] handBonesLens = { new float[] { 7.0f, 4f, 2.5f, 2.0f }, new float[] { 7f, 5f, 3f, 2f }, new float[] { 6, 4, 3, 2 }, new float[] { 6, 3.5f, 2.5f, 1.7f } };

        private float realityVirtualityRescaleFactor;

        private JointData[] palmBonesData = new JointData[FINGERS_NR];

        private Dictionary<FingerType, JointData[]> fingersJointsData = new Dictionary<FingerType, JointData[]>();                                      

        private Dictionary<Tuple<FingerType,FingerSection>, Vector3> bonesLookDirectionsInit = new Dictionary<Tuple<FingerType,FingerSection>, Vector3>();

        private Vector3 handJointPosVirtuality = new Vector3();

        private Dictionary<Tuple<FingerType, FingerSection>, Vector3> sensorsTranslations;

        private Dictionary<Tuple<FingerType, FingerSection>, Matrix4x4> sensorsRotsMats;

        private List<FingerType> fingersResolutionList;

        private Transform[,] jointsSensors = new Transform[FINGERS_NR, 4];

        private Transform[,] jointsAvatar = new Transform[FINGERS_NR, 4];

        private bool doStateRecording = false;

        private List<TransformsData> transformsData = new List<TransformsData>();

        public HandSolverBySensors(Transform handJoint, Dictionary<Tuple<FingerType, FingerSection>, Vector3> sensorsTranslations, Dictionary<Tuple<FingerType, FingerSection>, Matrix4x4> sensorsRotsMats, List<FingerType> fingersResolutionList) : base(handJoint)
        { 
            handJointPosVirtuality = handJoint.position;                                                          
            realityVirtualityRescaleFactor = 0;
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))
            {                
                Transform proximalJointTransform = handJoint.GetChild((int)fingerType).GetChild(0).GetChild(0);
                realityVirtualityRescaleFactor += 1.0f / FINGERS_NR *
                    Vector3.Distance(proximalJointTransform.position, proximalJointTransform.GetChild(0).GetChild(0).position) / (handBonesLens[(int)fingerType][2] + handBonesLens[(int)fingerType][3]);
                    //(sensorsPoses[new Tuple<FingerType,FingerSection>(fingerType, FingerSection.PROXIMAL)] - sensorsPoses[new Tuple<FingerType,FingerSection>(fingerType, FingerSection.DISTAL)]).magnitude;                             
            }
            
            foreach (FingerType fingerType in fingersResolutionList)
            {
                fingersJointsData.Add(fingerType, new JointData[4]);

                Transform jointTransform = handJoint.GetChild((int)fingerType);
                palmBonesData[(int)fingerType] = new JointData(handJoint, jointTransform.position, 0.0f);

                jointTransform = jointTransform.GetChild(0);
                // rescaling bone 0 by moving bone 1 (moving bone 1 is done by moving joint 0)
                float boneLenVirtuality = handBonesLens[(int)fingerType][0] * realityVirtualityRescaleFactor;
                //jointTransform.position = handJoint.position + (jointTransform.position - handJoint.position).normalized * boneLenVirtuality;
                // constructing joint 0 data
                fingersJointsData[fingerType][0] = new JointData(jointTransform.parent, jointTransform.position, (jointTransform.position - handJoint.position).magnitude);

                jointTransform = jointTransform.GetChild(0);
                // rescaling bone 1 by moving bone 2 (moving bone 2 is done by moving joint 1)

                boneLenVirtuality = handBonesLens[(int)fingerType][1] * realityVirtualityRescaleFactor;
                //jointTransform.position = fingersJointsData[fingerType][0].pos + (jointTransform.position - fingersJointsData[fingerType][0].pos).normalized * boneLenVirtuality;
                // constructing joint 1 data
                fingersJointsData[fingerType][1] = new JointData(jointTransform.parent, jointTransform.position, (jointTransform.parent.position - jointTransform.position).magnitude);                
                bonesLookDirectionsInit[new Tuple<FingerType, FingerSection>(fingerType, FingerSection.PROXIMAL)] = (fingersJointsData[fingerType][0].pos - fingersJointsData[fingerType][1].pos).normalized;

                jointTransform = jointTransform.GetChild(0);
                // rescaling bone 2 by moving bone 3 (moving bone 3 is done by moving joint 2)
                boneLenVirtuality = handBonesLens[(int)fingerType][2] * realityVirtualityRescaleFactor;
                //jointTransform.position = fingersJointsData[fingerType][1].pos + (jointTransform.position - fingersJointsData[fingerType][1].pos).normalized * boneLenVirtuality;
                // constructing joint 2 data
                fingersJointsData[fingerType][2] = new JointData(jointTransform.parent, jointTransform.position, boneLenVirtuality);                

                jointTransform = jointTransform.GetChild(0);
                // rescaling bone 3 by moving joint 3
                boneLenVirtuality = handBonesLens[(int)fingerType][3] * realityVirtualityRescaleFactor;                
                //jointTransform.position = fingersJointsData[fingerType][2].pos + (jointTransform.position - fingersJointsData[fingerType][2].pos).normalized * boneLenVirtuality;
                // constructing joint 3 data
                fingersJointsData[fingerType][3] = new JointData(jointTransform.parent, jointTransform.position, boneLenVirtuality);
                bonesLookDirectionsInit[new Tuple<FingerType, FingerSection>(fingerType, FingerSection.DISTAL)] = (fingersJointsData[fingerType][3].pos - fingersJointsData[fingerType][2].pos).normalized;           
            }

            this.sensorsTranslations = sensorsTranslations;
            this.sensorsRotsMats = sensorsRotsMats;
            this.fingersResolutionList = fingersResolutionList;

            for (int fingerIdx = 0; fingerIdx < FINGERS_NR; fingerIdx++) {
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    jointsSensors[fingerIdx, jointIdx] = GameObject.Find("j" + fingerIdx + jointIdx + "Sensor").GetComponent<Transform>();
                    jointsSensors[fingerIdx, jointIdx].gameObject.SetActive(false);
                    jointsAvatar[fingerIdx, jointIdx] = GameObject.Find("j" + fingerIdx + jointIdx + "Avatar").GetComponent<Transform>();
                    jointsAvatar[fingerIdx, jointIdx].gameObject.SetActive(false);
                }
            }
        }

        public void updateSensorsVals(Dictionary<Tuple<FingerType, FingerSection>, Vector3> sensorsTranslations, Dictionary<Tuple<FingerType, FingerSection>, Matrix4x4> sensorsRotsMats)
        {
            this.sensorsTranslations = sensorsTranslations;
            this.sensorsRotsMats = sensorsRotsMats;
        }

        public void updateFingersResolutionList(List<FingerType> fingersResolutionList)
        {
            this.fingersResolutionList = fingersResolutionList;
        }

        public void revertHandToBindPose()
        {
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))
            {
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                    fingersJointsData[fingerType][jointIdx].revertToInit();                
            }
        }

        public Dictionary<FingerType, float[]> simulateJointsSensors()
        {
            Dictionary<FingerType, float[]> biases = new Dictionary<FingerType, float[]>();

            foreach (FingerType fingerType in fingersResolutionList)
            {
                biases.Add(fingerType, new float[4]);

                // compute the new joints positions              
                Tuple<FingerType, FingerSection> fingerTypeSectionDuo;
                fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(fingerType, FingerSection.PROXIMAL);
                jointsSensors[(int)fingerType, 1].position = fingersJointsData[fingerType][1].posInit + sensorsTranslations[fingerTypeSectionDuo] * realityVirtualityRescaleFactor;
                jointsSensors[(int)fingerType, 0].position = jointsSensors[(int)fingerType, 1].position + sensorsRotsMats[fingerTypeSectionDuo].transpose.MultiplyVector(bonesLookDirectionsInit[fingerTypeSectionDuo]) * fingersJointsData[fingerType][1].tippedBoneLen;
                fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(fingerType, FingerSection.DISTAL);
                jointsSensors[(int)fingerType, 2].position = fingersJointsData[fingerType][2].posInit + sensorsTranslations[fingerTypeSectionDuo] * realityVirtualityRescaleFactor;
                jointsSensors[(int)fingerType, 3].position = jointsSensors[(int)fingerType, 2].position + sensorsRotsMats[fingerTypeSectionDuo].transpose.MultiplyVector(bonesLookDirectionsInit[fingerTypeSectionDuo]) * fingersJointsData[fingerType][3].tippedBoneLen;

                
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    jointsSensors[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsAvatar[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsAvatar[(int)fingerType, jointIdx].position = fingersJointsData[fingerType][jointIdx].tippedBoneTransform.GetChild(0).position;
                    biases[fingerType][jointIdx] = Vector3.Distance(jointsAvatar[(int)fingerType, jointIdx].position, jointsSensors[(int)fingerType, jointIdx].position);
                }
            }

            return biases;
        }

        public void alignJointsSensorsToJointsAvatars()
        {
            foreach (FingerType fingerType in fingersResolutionList)
            {
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    jointsSensors[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsAvatar[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsSensors[(int)fingerType, jointIdx].position = jointsAvatar[(int)fingerType, jointIdx].position;
                }
            }
        }

        public void disableJointsSensorsAndAvatars()
        {
            foreach (FingerType fingerType in fingersResolutionList)
            {
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    jointsSensors[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsAvatar[(int)fingerType, jointIdx].gameObject.SetActive(true);
                }
            }
        }                

        public override void resolve()
        {
            foreach (FingerType fingerType in fingersResolutionList)
            {
                // compute the new joints positions              
                Tuple<FingerType, FingerSection> fingerTypeSectionDuo;
                fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(fingerType, FingerSection.PROXIMAL);
                fingersJointsData[fingerType][1].posCache = fingersJointsData[fingerType][1].posInit + sensorsTranslations[fingerTypeSectionDuo] * realityVirtualityRescaleFactor;
                //fingersJointsData[fingerType][0].posCache = fingersJointsData[fingerType][1].posCache + sensorsLookDirections[fingerTypeSectionDuo] * fingersJointsData[fingerType][1].tippedBoneLen;
                fingersJointsData[fingerType][0].posCache = fingersJointsData[fingerType][1].posCache + sensorsRotsMats[fingerTypeSectionDuo].transpose.MultiplyVector(bonesLookDirectionsInit[fingerTypeSectionDuo]) * fingersJointsData[fingerType][1].tippedBoneLen;
                fingerTypeSectionDuo = new Tuple<FingerType, FingerSection>(fingerType, FingerSection.DISTAL);
                fingersJointsData[fingerType][2].posCache = fingersJointsData[fingerType][2].posInit + sensorsTranslations[fingerTypeSectionDuo] * realityVirtualityRescaleFactor;
                fingersJointsData[fingerType][3].posCache = fingersJointsData[fingerType][2].posCache + sensorsRotsMats[fingerTypeSectionDuo].transpose.MultiplyVector(bonesLookDirectionsInit[fingerTypeSectionDuo]) * fingersJointsData[fingerType][3].tippedBoneLen;
            }

            // animate the hand asset            
            Vector3 knucklesPosesCacheMean = new Vector3();
            Vector3 knucklesPosesMean = new Vector3();
            foreach (FingerType fingerTypesIt in Enum.GetValues(typeof(FingerType)))
            {
                knucklesPosesCacheMean += fingersJointsData[fingerTypesIt][0].posCache;
                knucklesPosesMean += fingersJointsData[fingerTypesIt][0].pos;
            }
            knucklesPosesCacheMean /= FINGERS_NR;
            knucklesPosesMean /= FINGERS_NR;

            Quaternion handJointRotDelta = Quaternion.FromToRotation((knucklesPosesMean - handJointPosVirtuality).normalized, (knucklesPosesCacheMean - handJointPosVirtuality).normalized);
            //palmBonesData[0].tippedBoneTransform.rotation = handJointRotDelta * palmBonesData[0].tippedBoneTransform.rotation;                                    
            foreach (FingerType fingerType in fingersResolutionList)
            {
                Quaternion rotAcc = handJointRotDelta;
                for (int jointIdx = 1; jointIdx < 4; jointIdx++)
                {
                    Vector3 fromVec = rotAcc * (fingersJointsData[fingerType][jointIdx].pos - fingersJointsData[fingerType][jointIdx - 1].pos).normalized;
                    Vector3 toVec = (fingersJointsData[fingerType][jointIdx].posCache - fingersJointsData[fingerType][jointIdx - 1].posCache).normalized;
                    Quaternion rotDelta = Quaternion.FromToRotation(fromVec, toVec);
                    fingersJointsData[fingerType][jointIdx - 1].pos = fingersJointsData[fingerType][jointIdx - 1].posCache;
                    fingersJointsData[fingerType][jointIdx].tippedBoneTransform.rotation = rotDelta * fingersJointsData[fingerType][jointIdx].tippedBoneTransform.rotation;

                    rotAcc = rotDelta * rotAcc;
                }
                fingersJointsData[fingerType][3].pos = fingersJointsData[fingerType][3].posCache;

                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    jointsSensors[(int)fingerType, jointIdx].gameObject.SetActive(true);
                    jointsSensors[(int)fingerType, jointIdx].position = fingersJointsData[fingerType][jointIdx].posCache;
                    jointsAvatar[(int)fingerType, jointIdx].gameObject.SetActive(false);
                }
            }

            if (doStateRecording)            
                transformsData.Add(new TransformsData(handJointTransform, DateTime.Now - recordingStartTime));            
        }

        public void startRecording()
        {
            doStateRecording = true;
            recordingStartTime = DateTime.Now;
        }

        public void pauseRecording()
        {
            doStateRecording = false;
        }

        public void resumeRecording()
        {
            doStateRecording = true;
        }

        public void saveTransformsDataToFile(string fullFilePath)
        {
            saveTransformsDataToFile(fullFilePath, transformsData);
        }        
    }
}
