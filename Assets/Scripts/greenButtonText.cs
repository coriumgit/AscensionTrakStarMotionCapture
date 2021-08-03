using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JasHandExperiment.Configuration;
using JasHandExperiment;

public class greenButtonText : MonoBehaviour {


    HandType handSide;

    // Use this for initialization
    void Start()
    {
        handSide = ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate;
        string num = "0";
        if (handSide.Equals(HandType.Left))
        {
            num = "3";
        }
        else if (handSide.Equals(HandType.Right))
        {
            num = "2";
        }
        TextMesh yelloeText = (TextMesh)gameObject.GetComponent(typeof(TextMesh));
        yelloeText.text = num;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
