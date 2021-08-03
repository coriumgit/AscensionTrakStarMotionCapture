using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JasHandExperiment
{
    /// <summary>
    /// The class handles animations for given object.
    /// it does that by mapping strings (reprentation of a animation trigger)
    /// to given boolean property name to change value.
    /// </summary>
    public class AnimationManager
    {
        #region Data Members
        /// <summary>
        /// The object which contains the animator
        /// </summary>
        private MonoBehaviour mOject;

        /// <summary>
        /// the animator that will control animating the object
        /// </summary>
        private Animator mAnimator;

        /// <summary>
        /// map string of trigger to string property name
        /// </summary>
        private Dictionary<string, string> mKeyPressToPropertyDict;

        /// <summary>
        /// enumerable of all of the properties relevant to the Animator
        /// </summary>
        private IEnumerable<string> mProperties;
        #endregion

        #region Ctors
        /// <summary>
        /// Constructor for AnumationManager.
        /// </summary>
        /// <param name="obj">the object that contains the Animator</param>
        /// <param name="properties">IEnumerable of all of the properties relevant to the animator</param>
        public AnimationManager(MonoBehaviour obj, IEnumerable<string> properties = null)
        {
            mOject = obj;
            mAnimator = mOject.GetComponent<Animator>();
            mProperties = properties;
        }

        /// <summary>
        /// Constructor for AnumationManager.
        /// </summary>
        /// <param name="obj">the object that contains the Animator</param>
        /// <param name="keyToPropMap">Dictionary that mapps external triggers (key press for example) to properties of animator</param>
        public AnimationManager(MonoBehaviour obj, Dictionary<string, string> keyToPropMap = null)
        {
            mOject = obj;
            mAnimator = mOject.GetComponent<Animator>();
            mKeyPressToPropertyDict = keyToPropMap;
            mProperties = mKeyPressToPropertyDict.Values;
        } 
        #endregion

        /// <summary>
        /// the function sets boolean value of given property
        /// </summary>
        /// <param name="propName">name of property to set</param>
        /// <param name="value">boolean value to set</param>
        public void SetBoolProperty(string propName, bool value)
        {
            mAnimator.SetBool(propName, value);
        }

        /// <summary>
        /// the fucntion sets 'value' to specific property and !valeue to all other properties.
        /// </summary>
        /// <param name="propName">property name to set to value</param>
        /// <param name="value">the value to set to relevant property</param>
        public void SetExclusivleyBoolPropery(string propName, bool value)
        {
            // uninitialized?
            if (mProperties == null)
            {
                Debug.Log("properties for animation manager weren't set and are null");
                return;
            }

            foreach (var prop in mProperties)
            {
                // found relevant property
                if (prop.Equals(propName))
                {
                    SetBoolProperty(prop, value);
                }
                else
                {
                    // all other properties are set to !value
                    SetBoolProperty(prop, !value);
                }
            }
        }

        /// <summary>
        /// the function sets false automatically to all properties
        /// </summary>
        public void SetFalseToAllProps()
        {
            // uninitialized?
            if (mProperties == null)
            {
                Debug.Log("properties for animation manager weren't set and are null");
                return;
            }
            foreach (var prop in mProperties)
            {
                SetBoolProperty(prop, false);    
            }
        }

        /// <summary>
        /// the fucntion respondes to a trigger exclusively.
        /// if the trigger is not mapped to any property, all properties are set to False.
        /// </summary>
        /// <param name="trigger">symbol of a trigger (key press for exmple)</param>
        public void RespondExclusivleyToTrigger(string trigger)
        {
            // uninitialized?
            if (mKeyPressToPropertyDict == null)
            {

                Debug.Log("keyToProperties dictionary for animation manager weren't set and are null");
            }

            // serach for relevant property and set exclusively
            if (mKeyPressToPropertyDict.ContainsKey(trigger))
            {
                string propToSet = mKeyPressToPropertyDict[trigger];
                SetExclusivleyBoolPropery(propToSet, true);
            }
            else
            {
                SetFalseToAllProps();
            }
        }

    }
}
