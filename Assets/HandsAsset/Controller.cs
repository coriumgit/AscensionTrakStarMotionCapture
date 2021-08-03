using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            anim.SetBool("LPinky", true);
            anim.SetBool("LRing", false);
            anim.SetBool("LMid", false);
            anim.SetBool("LInd", false);

        }
        else
        {
            if (Input.GetKey(KeyCode.Alpha2))
            {

                anim.SetBool("LInd", false);
                anim.SetBool("LPinky", false);
                anim.SetBool("LRing", true);
                anim.SetBool("LMid", false);
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                anim.SetBool("LPinky", false);
                anim.SetBool("LRing", false);
                anim.SetBool("LInd", false);
                anim.SetBool("LMid", true);
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                anim.SetBool("LInd", true);
                anim.SetBool("LPinky", false);
                anim.SetBool("LRing", false);
                anim.SetBool("LMid", false);
            }
            else
            {
                anim.SetBool("LInd", false);
                anim.SetBool("LPinky", false);
                anim.SetBool("LRing", false);
                anim.SetBool("LMid", false);
            }
        }
    }
}
