using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointLimits : MonoBehaviour
{
    // Start is called before the first frame update
    public float upperX;
    public float lowerX;
    public float upperY;
    public float lowerY;

    void Start()
    {
        upperX = convertDegsToTan(upperX);
        lowerX = convertDegsToTan(lowerX);
        upperY = convertDegsToTan(upperY);
        lowerY = convertDegsToTan(lowerY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static float convertDegsToTan(float deg)
    {
        return deg != 90.0f && deg != -90.0f ? Mathf.Tan(deg / 180.0f * Mathf.PI) : (deg == 90.0f ? Mathf.Infinity : -Mathf.Infinity);
    }    
}
