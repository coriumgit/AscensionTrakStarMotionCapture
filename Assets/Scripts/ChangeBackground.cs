using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JasHandExperiment.Configuration;
using UnityEngine.SceneManagement;

namespace JasHandExperiment
{
    public class ChangeBackground : MonoBehaviour
    {
        //Change the background according to genderType

        public Material[] material;
        void Start()
        {
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("testroom"))
            {
                if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandGender == GenderType.Male)
                {
                    if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == HandType.Left)
                        RenderSettings.skybox = material[0];
                    if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == HandType.Right)
                        RenderSettings.skybox = material[1];

                }
                else if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandGender == GenderType.Female)
                {
                    if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == HandType.Left)
                        RenderSettings.skybox = material[2];
                    if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate == HandType.Right)
                        RenderSettings.skybox = material[3];

                }
            }
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("calibScene")
                || SceneManager.GetActiveScene() == SceneManager.GetSceneByName("emptyRoom"))
            {
                if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandGender == GenderType.Female)
                {
                    RenderSettings.skybox = material[4];
                }
                else if (ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandGender == GenderType.Male)
                {
                    RenderSettings.skybox = material[5];
                }
            }
        }
    }
}