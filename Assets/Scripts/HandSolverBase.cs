using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

namespace JasHandExperiment
{
    public abstract class HandSolverBase
    {
        protected const float EPSILON = 0.001f;

        protected static readonly int FINGERS_NR = Enum.GetValues(typeof(FingerType)).Length;       

        [Serializable]
        protected class TransformsData
        {
            public TimeSpan dataTimeStamp;
            public SerializableQuaternion handJointRot;
            public SerializableVector3[] palmBonesPoses;
            public SerializableQuaternion[] palmBonesRots;
            public SerializableVector3[][] jointsPoses;
            public SerializableQuaternion[][] jointsRots;

            public TransformsData(Transform handJointTransform, TimeSpan dataTimeStamp)
            {
                handJointRot = handJointTransform.rotation;
                palmBonesPoses = new SerializableVector3[FINGERS_NR];
                palmBonesRots = new SerializableQuaternion[FINGERS_NR];
                jointsPoses = new SerializableVector3[FINGERS_NR][];
                jointsRots = new SerializableQuaternion[FINGERS_NR][];
                for (int fingerIdx = 0; fingerIdx < FINGERS_NR; fingerIdx++)
                {
                    Transform transformsIt = handJointTransform.GetChild(fingerIdx);
                    palmBonesPoses[fingerIdx] = transformsIt.position;
                    palmBonesRots[fingerIdx] = transformsIt.rotation;
                    jointsPoses[fingerIdx] = new SerializableVector3[4];
                    jointsRots[fingerIdx] = new SerializableQuaternion[4];
                    for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                    {
                        transformsIt = transformsIt.GetChild(0);
                        jointsPoses[fingerIdx][jointIdx] = transformsIt.position;
                        jointsRots[fingerIdx][jointIdx] = transformsIt.rotation;
                    }
                }

                this.dataTimeStamp = dataTimeStamp;
            }                    
        }

        protected Transform handJointTransform;

        private Transform[] fingersTipsTransforms = new Transform[FINGERS_NR];

        public HandSolverBase(Transform handJoint)
        {
            handJointTransform = handJoint;
            foreach (FingerType fingerType in Enum.GetValues(typeof(FingerType)))            
                fingersTipsTransforms[(int)fingerType] = handJoint.GetChild((int)fingerType).GetChild(0).GetChild(0).GetChild(0).GetChild(0);                                                                                         
        }

        public Vector3 getFingerTipPos(FingerType fingerType)
        {
            return fingersTipsTransforms[(int)fingerType].position;
        }

        public abstract void resolve();

        static protected void saveTransformsDataToFile(string fullFilePath, List<TransformsData> transformsData)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(fullFilePath);
            bf.Serialize(file, transformsData);
            file.Close();
        }

        protected void loadTransformsData(TransformsData transformsData)
        {
            Debug.Log("reloaded transform on: " + transformsData.dataTimeStamp.ToString());
            handJointTransform.rotation = transformsData.handJointRot;
            for (int fingerIdx = 0; fingerIdx < FINGERS_NR; fingerIdx++)
            {
                Transform transformsIt = handJointTransform.GetChild(fingerIdx);
                transformsIt.position = transformsData.palmBonesPoses[fingerIdx];
                transformsIt.rotation = transformsData.palmBonesRots[fingerIdx];
                for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                {
                    transformsIt = transformsIt.GetChild(0);
                    transformsIt.position = transformsData.jointsPoses[fingerIdx][jointIdx];
                    transformsIt.rotation = transformsData.jointsRots[fingerIdx][jointIdx];
                }
            }
        }
    }
}
