using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

namespace JasHandExperiment
{
    public class switchScene : MonoBehaviour
    {
        public AudioClip clip; // beep sound 
        public AudioSource source; // the source for beep sound 
        private uint interBlockTimeout, blocksAmount; // times according to configuration 
	    public Text text; // the text on the screen 
        private float timer, StartTime; //timers to change scenes
        private bool flag, flag2;	
        // Use this for initialization
        void Start()
        {
            source.clip = clip;
            flag2 = false;
            timer = ConfigurationManager.Instance.Configuration.SubRuns[0].InterBlockTimeout;
            StartTime = Time.time;
            flag = false;
			interBlockTimeout = ConfigurationManager.Instance.Configuration.SubRuns[0].InterBlockTimeout;
			blocksAmount = ConfigurationManager.Instance.Configuration.SubRuns[0].BlocksAmount;
        }

        // Update is called once per frame
        void Update()
        {
            if (CommonConstants.FirstRun == true) {
                text.text = "To see the experiment room \n Click SPACE ";
				if (Input.GetKeyDown (KeyCode.Return)) {
                    CommonConstants.FirstRun = false;
                    SceneManager.LoadScene("emptyRoom");
                }
            }			
        }
    }
}