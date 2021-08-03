using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonTools;
using System.IO;

public class SimManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject j0;
    public GameObject j1;
    public GameObject j2;
    public GameObject j3;
    public GameObject jt;

    private Transform[] jointsTransforms = new Transform[4];
    private Transform targetTransform;

    List<Vector3[]> jointsPosesPerIKIteration = new List<Vector3[]>();

    private int currIKIteration = 0;
    private int currIKIterationMoveIdx = 3;

    void Start()
    {
        jointsTransforms[0] = j0.GetComponent<Transform>();
        jointsTransforms[1] = j1.GetComponent<Transform>();
        jointsTransforms[2] = j2.GetComponent<Transform>();
        jointsTransforms[3] = j3.GetComponent<Transform>();
        targetTransform = jt.GetComponent<Transform>();

        CSVFile mReadFile = new CSVFile();               
        // init the file to write to
        mReadFile.Init(System.Environment.CurrentDirectory + "\\IKDebug.csv", FileMode.Open, ',');
        mReadFile.ReadLine();
        foreach (string[] IKDataLine in mReadFile.ReadLines()) {            
            Vector3[] jointsPosesByLine = new Vector3[5];
            jointsPosesByLine[0] = new Vector3(float.Parse(IKDataLine[0]), float.Parse(IKDataLine[1]), float.Parse(IKDataLine[2]));
            jointsPosesByLine[1] = new Vector3(float.Parse(IKDataLine[3]), float.Parse(IKDataLine[4]), float.Parse(IKDataLine[5]));
            jointsPosesByLine[2] = new Vector3(float.Parse(IKDataLine[6]), float.Parse(IKDataLine[7]), float.Parse(IKDataLine[8]));
            jointsPosesByLine[3] = new Vector3(float.Parse(IKDataLine[9]), float.Parse(IKDataLine[10]), float.Parse(IKDataLine[11]));
            jointsPosesByLine[4] = new Vector3(float.Parse(IKDataLine[12]), float.Parse(IKDataLine[13]), float.Parse(IKDataLine[14]));
            jointsPosesPerIKIteration.Add(jointsPosesByLine);
        }

        Vector3[] jointsPosesCurr = jointsPosesPerIKIteration[0];
        jointsTransforms[0].position = jointsPosesCurr[0];
        jointsTransforms[1].position = jointsPosesCurr[1];
        jointsTransforms[2].position = jointsPosesCurr[2];
        jointsTransforms[3].position = jointsPosesCurr[3];
        targetTransform.position = jointsPosesCurr[3];        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && (currIKIteration < jointsPosesPerIKIteration.Count - 1 || currIKIterationMoveIdx < 3))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
            {
                currIKIterationMoveIdx = 3;
                if (Input.GetKey(KeyCode.LeftShift))
                    currIKIteration = Mathf.Min(currIKIteration + 10 + ((currIKIteration + 1) % 2), jointsPosesPerIKIteration.Count - 2);
                else
                    currIKIteration = Mathf.Min(currIKIteration + 50 + ((currIKIteration + 1) % 2), jointsPosesPerIKIteration.Count - 2);
            }
            else
            {
                if (currIKIteration % 2 == 1)
                {
                    if (currIKIterationMoveIdx == 0)
                        currIKIteration++;
                    else
                        currIKIterationMoveIdx--;
                }
                else
                {
                    if (currIKIterationMoveIdx == 3)
                        currIKIteration++;
                    else
                        currIKIterationMoveIdx++;
                }
            }
            
            updateJointsPoses();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && (currIKIteration > 0 || currIKIterationMoveIdx > 0))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
            {
                currIKIterationMoveIdx = 3;
                if (Input.GetKey(KeyCode.LeftShift))
                    currIKIteration = Mathf.Max(0, currIKIteration - 10 - ((currIKIteration + 1) % 2));
                else
                    currIKIteration = Mathf.Max(0, currIKIteration - 50 - ((currIKIteration + 1) % 2));
            }
            else
            {
                if (currIKIteration % 2 == 1)
                {
                    if (currIKIterationMoveIdx == 3)
                        currIKIteration--;
                    else
                        currIKIterationMoveIdx++;
                }
                else
                {
                    if (currIKIterationMoveIdx == 0)
                        currIKIteration--;
                    else
                        currIKIterationMoveIdx--;
                }
            }

            updateJointsPoses();
        }

        Vector3[] jointsPoses = new Vector3[4];
        for (int i = 0; i < 4; i++)
            jointsPoses[i] = jointsTransforms[i].position;
        int j = 0;
    }

    private void updateJointsPoses()
    {
        Vector3[] jointsPosesCurr = jointsPosesPerIKIteration[currIKIteration];
        jointsTransforms[currIKIterationMoveIdx].position = jointsPosesCurr[currIKIterationMoveIdx];
        targetTransform.position = jointsPosesCurr[4];        
        Debug.Log("Iteration #" + currIKIteration + "; Joint #" + currIKIterationMoveIdx);        
    }
}
