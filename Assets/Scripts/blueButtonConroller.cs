using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class blueButtonConroller : MonoBehaviour {

    public bool isPressed = true;
    private Rigidbody button;

	// Use this for initialization
	void Start () {
  //      button = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixUpdate () {
		if (isPressed == true)
        {
            //Vector3 downMove = new Vector3(x: 1,y: 10,z: 1);
            //button.AddForce(downMove);
          //  button.MovePosition(button.position + downMove * Time.deltaTime);
        //    Vector3 moveUp = new Vector3(x: -2.2f,y: 0.5f,z: 0.2f);
        //   button.AddForce(moveUp);
        //    isPressed = false;
        }
	}
}
