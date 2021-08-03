using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace JasHandExperiment
{
    public class moveToCalibration : MonoBehaviour
    {
        //change scene from empty room to calibration room  
        public Text text;
        // Use this for initialization
        void Start()
        {   
			// i put all in comment because Jas wanted to do the caliberation no metter if we are on active type or not
			//	if you want to do the calibration in the active mode only, remove the comments


            //the text on the display once it runs . 
        //    if(ConfigurationManager.Instance.Configuration.ExperimentType == Configuration.ExperimentType.Active)
                 text.text = "Click Space To Move To The Test Room";
         //   else
          //      text.text = "Click Space To Move To Test Room";
        }

        // Update is called once per frame
        void Update()
        {

		// i put all in comment because Jas wanted to do the caliberation no metter if we are on active type or not
		//	if you want to do the calibration in the active mode only, remove the comments

         //   //the key to change scenes 
           // if (ConfigurationManager.Instance.Configuration.ExperimentType == Configuration.ExperimentType.Active)
            //{
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SceneManager.LoadScene("testRoom");
                }
            //}
            //else
            //{
             //   if (Input.GetKeyDown(KeyCode.Space))
             //   {
             //       SceneManager.LoadScene("testroom");
              //  }
            //}

        }
    }
}
