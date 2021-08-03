using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace JasHandExperiment
{
    public class startExperiment : MonoBehaviour
    {
        public Text text;
        // Use this for initialization
        void Start()
        {
            text.text = "Click Space To Start Experiment";
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("testroom");
            }

        }
    }
}
