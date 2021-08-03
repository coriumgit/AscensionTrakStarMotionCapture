using UnityEngine;
using System;
using System.Collections.Generic;
using CommonTools;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JasHandExperiment
{
    public class IKSolver
    {
        #region Nested Types       
        private class JointData
        {            
            public Vector3 jointPos { get; internal set; } // joint global position
            public float tippedBoneLen { get; internal set; } // The length of the tipped bone of which this joint is it's tip
            public Transform tippedBoneTransform { get; internal set; } // The transform of the bone of which this joint is it's tip
            public JointLimits jointLimits { get; internal set; } // joint angles limits (force parent joint inside the cone defined by these angles)
            public JointData(Vector3 _jointPos, float _tippedBoneLen, Transform _tippedBoneTransform, JointLimits _jointLimits)
            {                
                jointPos = _jointPos;
                tippedBoneLen = _tippedBoneLen;
                tippedBoneTransform = _tippedBoneTransform;
                jointLimits = _jointLimits;
            }
        }

        // Reminder: A chain hierarchy node includes (in graph theory terms) A vertices chain (A vertex encapsulates JointData) that ends with either a sub base or with an end effector
        private class ChainsHierarchyNode
        {
            public ChainsHierarchyNode parentChain { get; internal set; }
            public float chainLen { get; internal set; }
            public List<ChainsHierarchyNode> childrenChains { get; internal set; }
            public List<JointData> jointsData { get; internal set; }
            public Vector3[] jointsPosesCache { get; set; }
             
            public ChainsHierarchyNode(List<JointData> _jointsData)
            {
                parentChain = null;
                childrenChains = new List<ChainsHierarchyNode>();
                jointsData = _jointsData;
                jointsPosesCache = new Vector3[jointsData.Count];
                for (int jointIdx = 0; jointIdx < jointsData.Count; jointIdx++)
                    jointsPosesCache[jointIdx] = jointsData[jointIdx].jointPos;

                chainLen = 0.0f;
                foreach (JointData jointData in jointsData)
                    chainLen += jointData.tippedBoneLen;
            }

            public void linkChild(ChainsHierarchyNode node)
            {
                node.parentChain = this;
                childrenChains.Add(node);
                node.jointsData[0].tippedBoneLen = Vector3.Distance(node.jointsData[0].jointPos, jointsData[jointsData.Count - 1].jointPos);
            }
        }

        #endregion

        #region Data Members

        private const float RESOLVE_ITERATIONS_NR_MAX = 10;

        private const float RESOLVE_TOLERANCE = 0.001f;

        private const float FIND_ROOT_BISECTION_ITERATIONS_NR_MAX = 20;

        private Vector3 handJointPosInit;

        private Quaternion handJointRotInit;                

        private List<ChainsHierarchyNode> endEffectorChains;

        private ChainsHierarchyNode rootChain;

        private Vector3 debugTarget;

        private CSVFile mWriteFile;

        private Transform[] jointsMarkers = new Transform[4];

        #endregion

        #region Methods

        public IKSolver(Transform rootUnityTransform)
        {
            if (rootUnityTransform.childCount == 0)
                throw new Exception("Hierarchy has only 1 node !");            

            endEffectorChains = new List<ChainsHierarchyNode>();

            rootChain = genHierarchy(rootUnityTransform);
            //foreach (Transform endEffectorTransform in endEffectorTransforms)
            //    targets.Add(endEffectorTransform.position);
            
            mWriteFile = new CSVFile();            
            string[] columns = {"0x", "0y", "0z", "1x", "1y", "1z", "2x", "2y", "2z", "3x", "3y", "3z", "tx", "ty", "tz"};
            var settings = new BatchCSVRWSettings();
            settings.WriteBatchSize = 1000;
            // interval?
            settings.WriteBatchDelayMsec = 1000 * 5;
            // init the file to write to
            mWriteFile.Init(System.Environment.CurrentDirectory + "\\IKSim\\" + "IKDebug.csv", FileMode.Create, ',', columns, settings);

            jointsMarkers[0] = GameObject.Find("j0Marker").GetComponent<Transform>();
            jointsMarkers[1] = GameObject.Find("j1Marker").GetComponent<Transform>();
            jointsMarkers[2] = GameObject.Find("j2Marker").GetComponent<Transform>();
            jointsMarkers[3] = GameObject.Find("j3Marker").GetComponent<Transform>();
                        
            string[] jointsPosesRec = new string[15];
            for (int jointIdx = 0; jointIdx < 4; jointIdx++)
            {
                Vector3 jointPos = endEffectorChains[0].jointsData[jointIdx].jointPos;
                jointsPosesRec[3 * jointIdx] = (jointPos.x - rootChain.jointsData[0].jointPos.x).ToString();
                jointsPosesRec[3 * jointIdx + 1] = (jointPos.y - rootChain.jointsData[0].jointPos.y).ToString();
                jointsPosesRec[3 * jointIdx + 2] = (jointPos.z - rootChain.jointsData[0].jointPos.z).ToString();
                jointsMarkers[jointIdx].position = jointPos;
            }
            jointsPosesRec[12] = "0.0";
            jointsPosesRec[13] = "0.0";
            jointsPosesRec[14] = "0.0";
            mWriteFile.WriteLine(jointsPosesRec);
        }

        public void close()
        {
            mWriteFile.Close();
        }

        private ChainsHierarchyNode genHierarchy(Transform nodeUnityTransform)
        {
            List<JointData> jointsData = new List<JointData>();
            Transform nodeUnityTransformIt = nodeUnityTransform;
            Transform nodeUnityTransformItPrev;
            if (nodeUnityTransform.parent != null)
                nodeUnityTransformItPrev = nodeUnityTransform.parent;
            else
                nodeUnityTransformItPrev = nodeUnityTransform;
            while (nodeUnityTransformIt.childCount == 1)
            {
                if (!nodeUnityTransformIt.gameObject.name.Contains("bone"))
                {
                    jointsData.Add(new JointData(nodeUnityTransformIt.position, 
                                                 Vector3.Distance(nodeUnityTransformIt.position, nodeUnityTransformItPrev.position),
                                                 nodeUnityTransformItPrev.transform, 
                                                 nodeUnityTransformIt.gameObject.GetComponent<JointLimits>()));
                    nodeUnityTransformItPrev = nodeUnityTransformIt;
                }

                nodeUnityTransformIt = nodeUnityTransformIt.GetChild(0);
            }
            jointsData.Add(new JointData(nodeUnityTransformIt.position,
                                         Vector3.Distance(nodeUnityTransformIt.position, nodeUnityTransformItPrev.position),
                                         nodeUnityTransformItPrev.transform,
                                         nodeUnityTransformIt.gameObject.GetComponent<JointLimits>()));

            ChainsHierarchyNode newChainNode = new ChainsHierarchyNode(jointsData);
            
            if (nodeUnityTransformIt.childCount == 0)
                endEffectorChains.Add(newChainNode);
            else
            {
                for (int childIdx = 0; childIdx < nodeUnityTransformIt.childCount; childIdx++)
                    newChainNode.linkChild(genHierarchy(nodeUnityTransformIt.GetChild(childIdx)));
            }

            return newChainNode;
        }

        public void resolve(Vector3[] targets)
        {
            debugTarget = targets[0];
            Vector3 b = rootChain.jointsData[0].jointPos;
            Vector3[] prevEndEffectorPoses = new Vector3[targets.Length];
            int iterationIdx = 0;
            //Debug.Log("---------------------ALGO-----------------------");
            //Debug.Log("target: " + (debugTarget - b));
            do
            {                
                for (int endEffectorIdx = 0; endEffectorIdx < targets.Length; endEffectorIdx++)
                {
                    ChainsHierarchyNode endEffectorChain = endEffectorChains[endEffectorIdx];
                    prevEndEffectorPoses[endEffectorIdx] = endEffectorChain.jointsPosesCache[endEffectorChain.jointsData.Count - 1];
                    endEffectorChain.jointsPosesCache[endEffectorChain.jointsData.Count - 1] = targets[endEffectorIdx];
                }
                //Debug.Log("-------------------ITERATION #" + iterationIdx + "--------------------");
                IKForwardReaching(rootChain, true);
                rootChain.jointsPosesCache[0] = b;                
                IKBackwardReaching(rootChain, true);
            } while (iterationIdx++ < RESOLVE_ITERATIONS_NR_MAX && !areEndEffectorsCloseEnough(targets, prevEndEffectorPoses));
            //Debug.Log("---------------------ALGO-----------------------");
            for (int jointIdx = 0; jointIdx < 4; jointIdx++)
                jointsMarkers[jointIdx].position = endEffectorChains[0].jointsPosesCache[jointIdx];
            updateTransforms(rootChain, new Quaternion(0.0f, 0.0f, 0.0f, 1.0f));
        }
        
        private void IKForwardReaching(ChainsHierarchyNode chainNode, bool debug)
        {
            if (chainNode.childrenChains.Count > 0)
            {
                debug = true;
                foreach (ChainsHierarchyNode childChain in chainNode.childrenChains)
                {
                    IKForwardReaching(childChain, debug);
                    debug = false;
                }
                                
                Vector3 subBaseTailJointComputedPosesSum = new Vector3(0.0f, 0.0f, 0.0f);
                foreach (ChainsHierarchyNode childChain in chainNode.childrenChains)
                {
                    Vector3 childHeadJointPosUpdated = childChain.jointsPosesCache[0];
                    Vector3 subBaseTailJointPosUpdated = chainNode.jointsPosesCache[chainNode.jointsData.Count - 1];
                    if (chainNode.jointsData[chainNode.jointsData.Count - 1].jointLimits != null)
                        subBaseTailJointPosUpdated = fixJointPosToLimits(subBaseTailJointPosUpdated, childHeadJointPosUpdated, childChain.jointsPosesCache[1], chainNode.jointsData[chainNode.jointsData.Count - 1].jointLimits);

                    float d = childChain.jointsData[0].tippedBoneLen;
                    float lambda = d / Vector3.Distance(subBaseTailJointPosUpdated, childHeadJointPosUpdated);
                    subBaseTailJointComputedPosesSum += (1 - lambda) * childHeadJointPosUpdated + lambda * subBaseTailJointPosUpdated;
                }
                chainNode.jointsPosesCache[chainNode.jointsData.Count - 1] = subBaseTailJointComputedPosesSum / chainNode.childrenChains.Count;
            }

            List<JointData> jointsData = chainNode.jointsData;
            for (int jointIdx = jointsData.Count - 2; jointIdx >= 0; jointIdx--)
            {                                               
                Vector3 backJointPosUpdated = chainNode.jointsPosesCache[jointIdx + 1];
                Vector3 jointPosUpdated = chainNode.jointsPosesCache[jointIdx];
 //               if (jointsData[jointIdx + 1].jointLimits != null)                                    
//                    jointPosUpdated = fixJointPosToLimits(jointPosUpdated, backJointPosUpdated, chainNode.jointsPosesCache[jointIdx + 2], jointsData[jointIdx + 1].jointLimits);
                
                float d = jointsData[jointIdx + 1].tippedBoneLen;
                float lambda = d / Vector3.Distance(jointPosUpdated, backJointPosUpdated);
                chainNode.jointsPosesCache[jointIdx] = (1 - lambda) * backJointPosUpdated + lambda * jointPosUpdated;               
            }

            if (debug)
                writeJointsChainToFile(chainNode);
        }        

        private void IKBackwardReaching(ChainsHierarchyNode chainNode, bool debug)
        {
            List<JointData> jointsData = chainNode.jointsData;
            for (int jointIdx = 1; jointIdx <= jointsData.Count - 1; jointIdx++)
            {
                Vector3 jointPosUpdated = chainNode.jointsPosesCache[jointIdx];
                Vector3 foreJointPosUpdated = chainNode.jointsPosesCache[jointIdx - 1];
//                if (chainNode.jointsData[jointIdx - 1].jointLimits != null)
 //                   jointPosUpdated = fixJointPosToLimits(jointPosUpdated, foreJointPosUpdated, chainNode.jointsData[jointIdx - 1].tippedBoneTransform.position, chainNode.jointsData[jointIdx - 1].jointLimits);
                float d = jointsData[jointIdx].tippedBoneLen;
                float lambda = d / Vector3.Distance(jointPosUpdated, foreJointPosUpdated);
                chainNode.jointsPosesCache[jointIdx] = (1 - lambda) * foreJointPosUpdated + lambda * jointPosUpdated;                
            }

            if (debug)
                writeJointsChainToFile(chainNode);

            debug = true;
            if (chainNode.childrenChains.Count > 0)
            {
                Vector3 subBaseJointPosUpdate = chainNode.jointsPosesCache[jointsData.Count - 1];                
                foreach (ChainsHierarchyNode childChain in chainNode.childrenChains)
                {
                    Vector3 childChainHeadJointPosUpdated = childChain.jointsPosesCache[0];                    
                    if (chainNode.jointsData[jointsData.Count - 1].jointLimits != null)
                        childChainHeadJointPosUpdated = fixJointPosToLimits(childChainHeadJointPosUpdated, subBaseJointPosUpdate, chainNode.jointsData[jointsData.Count - 1].tippedBoneTransform.position, chainNode.jointsData[jointsData.Count - 1].jointLimits);
                    float d = childChain.jointsData[0].tippedBoneLen;
                    float lambda = d / Vector3.Distance(childChainHeadJointPosUpdated, subBaseJointPosUpdate);
                    childChain.jointsPosesCache[0] = (1 - lambda) * subBaseJointPosUpdate + lambda * childChainHeadJointPosUpdated;
                    //if (debug)
                    //    Debug.Log("joint #0 pos: " + (childChain.jointsPosesCache[0] - rootChain.jointsData[0].jointPos));
                    IKBackwardReaching(childChain, debug);
                    debug = false;
                }
            }
        }

        private void updateTransforms(ChainsHierarchyNode chainNode, Quaternion rotAcc)
        {
            List<JointData> jointsData = chainNode.jointsData;
            //jointsData[0].jointPos = chainNode.jointsPosesCache[0];
            for (int jointIdx = 1; jointIdx < jointsData.Count; jointIdx++)
            {                
                Quaternion rotDelta = Quaternion.FromToRotation((jointsData[jointIdx].jointPos - jointsData[jointIdx - 1].jointPos), chainNode.jointsPosesCache[jointIdx] - chainNode.jointsPosesCache[jointIdx - 1]);
                //jointData.tippedBoneTransform.rotation = rotDelta * jointData.tippedBoneTransform.rotation;
                //rotAcc = rotDelta * rotAcc;
                updateParentTransformIndependantOfChildren(jointsData[jointIdx].tippedBoneTransform, chainNode.jointsPosesCache[jointIdx - 1], rotDelta * jointsData[jointIdx].tippedBoneTransform.rotation);
                jointsData[jointIdx - 1].jointPos = chainNode.jointsPosesCache[jointIdx - 1];
            }
            jointsData[jointsData.Count - 1].jointPos = chainNode.jointsPosesCache[jointsData.Count - 1];

            if (chainNode.childrenChains.Count > 0)
            {
                Quaternion[] subBaseChildrenBonesQuats = new Quaternion[chainNode.childrenChains.Count];                
                for (int childChainIdx = 0; childChainIdx < chainNode.childrenChains.Count; childChainIdx++)                                    
                    subBaseChildrenBonesQuats[childChainIdx] = Quaternion.FromToRotation(rotAcc*(chainNode.childrenChains[childChainIdx].jointsData[0].jointPos - chainNode.jointsData[jointsData.Count - 1].jointPos), chainNode.childrenChains[childChainIdx].jointsPosesCache[0] - chainNode.jointsPosesCache[jointsData.Count - 1]);

                Quaternion rotDelta = subBaseChildrenBonesQuats[0]; // quatsMean(subBaseChildrenBonesQuats);
                Transform subBaseTransform = chainNode.childrenChains[0].jointsData[0].tippedBoneTransform;
                subBaseTransform.rotation = rotDelta * subBaseTransform.rotation;
                rotAcc = rotDelta * rotAcc;
                chainNode.jointsData[jointsData.Count - 1].jointPos = chainNode.jointsPosesCache[jointsData.Count - 1];

                foreach (ChainsHierarchyNode childChain in chainNode.childrenChains)
                    updateTransforms(childChain, rotAcc);
            }            
        }

        private void updateParentTransformIndependantOfChildren(Transform transform, Vector3 pos, Quaternion rot)
        {
            if (transform.childCount > 0)
            {
                Vector3[] childrenPoses = new Vector3[transform.childCount];
                Quaternion[] childrenQuats = new Quaternion[transform.childCount];                
                for (int childIdx = 0; childIdx < transform.childCount; childIdx++)
                {
                    childrenPoses[childIdx] = transform.GetChild(childIdx).position;
                    childrenQuats[childIdx] = transform.GetChild(childIdx).rotation;
                }
                transform.position = pos;
                transform.rotation = rot;
                for (int childIdx = 0; childIdx < transform.childCount; childIdx++)
                {
                    transform.GetChild(childIdx).position = childrenPoses[childIdx];
                    transform.GetChild(childIdx).rotation = childrenQuats[childIdx];
                }
            }
            else
                transform.rotation = rot;
        }
        
        private bool areEndEffectorsCloseEnough(Vector3[] targets, Vector3[] prevEndEffectorPoses)
        {
            for (int endEffectorIdx = 0; endEffectorIdx < targets.Length; endEffectorIdx++)
            {
                Vector3 endEffectorPos = endEffectorChains[endEffectorIdx].jointsPosesCache[endEffectorChains[endEffectorIdx].jointsData.Count - 1];
                if (Vector3.Distance(endEffectorPos, targets[endEffectorIdx]) > RESOLVE_TOLERANCE &&
                    Vector3.Distance(endEffectorPos, prevEndEffectorPoses[endEffectorIdx]) > RESOLVE_TOLERANCE)
                    return false;
            }

            return true;
        }

        private void writeJointsChainToFile(ChainsHierarchyNode chainNode)
        {
            string[] jointsPosesRec = new string[15];
            for (int jointIdx = 0; jointIdx < 4; jointIdx++)
            {
                jointsPosesRec[3 * jointIdx] = (chainNode.jointsPosesCache[jointIdx].x - rootChain.jointsData[0].jointPos.x).ToString();
                jointsPosesRec[3 * jointIdx + 1] = (chainNode.jointsPosesCache[jointIdx].y - rootChain.jointsData[0].jointPos.y).ToString();
                jointsPosesRec[3 * jointIdx + 2] = (chainNode.jointsPosesCache[jointIdx].z - rootChain.jointsData[0].jointPos.z).ToString();
            }
            jointsPosesRec[12] = (debugTarget.x - rootChain.jointsData[0].jointPos.x).ToString();
            jointsPosesRec[13] = (debugTarget.y - rootChain.jointsData[0].jointPos.y).ToString();
            jointsPosesRec[14] = (debugTarget.z - rootChain.jointsData[0].jointPos.z).ToString();
            mWriteFile.WriteLine(jointsPosesRec);
        }

        private static Vector3 fixJointPosToLimits(Vector3 jointPos, Vector3 limitingJointPos, Vector3 precLimitingJointPos, JointLimits jointLimits)
        {
            // A -> back back joint position
            // B -> back joint Position
            // C -> current joint position
            // O -> projection of C on the line spanned by A and B
            // since O is on AB: O = A + t*AB = t*B + (1 - t)*A, for some t in R 
            // since OC _|_ AB: t = AB*AC / |AB|^2
            Vector3 AB = limitingJointPos - precLimitingJointPos;
            Vector3 AC = jointPos - precLimitingJointPos;
            float t = Vector3.Dot(AB, AC) / AB.sqrMagnitude;
            Vector3 O = t * limitingJointPos + (1 - t) * precLimitingJointPos;
            Quaternion OFrameRot = Quaternion.FromToRotation(AB, Vector3.forward);
            Vector3 jointPosRel = OFrameRot * (jointPos - O);            
            float s = Vector3.Magnitude(O - limitingJointPos);
            float qx, qy; // the parameters of the ellipse corresponding to the joint angles limits
            if (jointPosRel.x > 0.0f)
                qx = s * jointLimits.upperX;
            else
                qx = s * jointLimits.lowerX;

            if (jointPosRel.y > 0.0f)
                qy = s * jointLimits.upperY;
            else
                qy = s * jointLimits.lowerY;

            if (qx == 0.0f && qy == 0.0f)
                jointPosRel.x = jointPosRel.y = 0;
            else if (qx == 0)
            {
                jointPosRel.x = 0;
                if (Mathf.Abs(jointPosRel.y) > Mathf.Abs(qy))
                    jointPosRel.y = qy;
            }
            else if (qy == 0)
            {
                jointPosRel.y = 0;
                if (Mathf.Abs(jointPosRel.x) > Mathf.Abs(qx))
                    jointPosRel.x = qx;
            }
            else if (1.0f < Mathf.Pow(jointPosRel.x, 2) / Mathf.Pow(qx, 2) + Mathf.Pow(jointPosRel.y, 2) / Mathf.Pow(qy, 2))
            {
                Vector2 jointPosRelCorrected = getClosestPointToEllipse(Mathf.Abs(qx), Mathf.Abs(qy), new Vector2(Mathf.Abs(jointPosRel.x), Math.Abs(jointPosRel.y)));
                jointPosRel.x = jointPosRel.x > 0.0f ? jointPosRelCorrected.x : -jointPosRelCorrected.x;
                jointPosRel.y = jointPosRel.y > 0.0f ? jointPosRelCorrected.y : -jointPosRelCorrected.y;
            }

            return Quaternion.Inverse(OFrameRot) * jointPosRel + O;
        }

        private static Vector2 getClosestPointToEllipse(float ellipseA, float ellipseB, Vector2 point)
        {
            if (ellipseA == Mathf.Infinity)            
                return new Vector3(point.x, ellipseB);                            
            else if (ellipseB == Mathf.Infinity)            
                return new Vector3(ellipseA, point.y);
            
            float e0, e1, x0, x1;
            bool isAxesFlipRequired = ellipseA < ellipseB;
            if (isAxesFlipRequired)
            {
                e0 = ellipseB;
                e1 = ellipseA;
            }
            else
            {
                e0 = ellipseA;
                e1 = ellipseB;
            }

            if (point.y > 0.0f)
            {
                if (point.x > 0.0f)
                {
                    float z0 = point.x / e0;
                    float z1 = point.y / e1;
                    float g = z0 * z0 + z1 * z1 - 1;
                    if (Mathf.Abs(g) > 0.001)
                    {                        
                        float r0 = e0 / e1 * (e0 / e1);
                        // find the root of F(t) = (e0*point.x/(t + e0^2))^2 + (e1*point.y/(t + e1^2))^2 - 1 = 0
                        // F(t) is formed by substituting the solution for (x0,x1) of:
                        //      point - (x0,x1) = t * gradient_of_implicit_equation_of_ellipse / 2
                        // in implicit_equation_of_ellipse
                        float t = findRootBisection(r0, z0, z1, g);
                        x0 = r0 * point.x / (t + r0);
                        x1 = point.y / (t + 1);
                    }
                    else
                    {
                        x0 = point.x;
                        x1 = point.y;
                    }
                }
                else
                { // point.x == 0                
                    x0 = 0.0f;
                    x1 = e1;
                }
            }
            else // point.y == 0
            {
                float e0e0 = e0 * e0;
                float e1e1 = e1 * e1;
                if (point.x < (e0e0 - e1e1) / e0)
                {
                    x0 = e0e0 * point.x / (e0e0 - e1e1);
                    x1 = e1 * Mathf.Sqrt(1 - x0 * x0 / e0e0);
                }
                else
                {
                    x0 = e0;
                    x1 = 0.0f;
                }
            }

            if (isAxesFlipRequired)
                return new Vector3(x1, x0);
            else
                return new Vector3(x0, x1);
        }

        private static float findRootBisection(float r0, float z0, float z1, float g)
        {
            float n0 = r0 * z0;
            float s0 = z1 - 1;
            float s1 = g < 0 ? 0 : new Vector2(n0, z1).magnitude - 1;
            float s = 0;
            for (int i = 0; i < FIND_ROOT_BISECTION_ITERATIONS_NR_MAX; i++)
            {
                s = (s0 + s1) / 2;
                if (s == s0 || s == s1)
                    break;

                float ratio0 = n0 / (s + r0);
                float ratio1 = z1 / (s + 1);
                g = ratio0 * ratio0 + ratio1 * ratio1 - 1;
                if (g > 0.0f)
                    s0 = s;
                else if (g < 0)
                    s1 = s;
                else
                    break;
            }

            return s;
        }

        private static Quaternion quatsMean(Quaternion[] quats) {
            //Global variable which 
            int quatsAcc = 0; //
            Quaternion addedRotation = Quaternion.identity;
            float x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f;
            foreach (Quaternion singleRotation in quats) {         
                float addDet = 1.0f / ++quatsAcc;
                addedRotation.w += singleRotation.w;
                w = addedRotation.w * addDet;
                addedRotation.x += singleRotation.x;
                x = addedRotation.x * addDet;
                addedRotation.y += singleRotation.y;
                y = addedRotation.y * addDet;
                addedRotation.z += singleRotation.z;
                z = addedRotation.z * addDet;

                //Normalize. Note: experiment to see whether you
                //can skip this step.
                float D = 1.0f / (w * w + x * x + y * y + z * z);
                w *= D;
                x *= D;
                y *= D;
                z *= D;                                
            }

            return new Quaternion(x, y, z, w);
        }        

        #endregion
    }
}