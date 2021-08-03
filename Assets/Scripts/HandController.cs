using System.Collections.Generic;
using CommonTools;
using UnityEngine;
using JasHandExperiment;
using System;
using JasHandExperiment.Configuration;
using static JasHandExperiment.ExperimentManager;

/// <summary>
/// The class is the controller of a hand in the experiment.
/// it controlls all aspects of the hand (visual state, movement, writing to files etc...)
/// </summary>
public class HandController : MonoBehaviour {    
    #region Data Members

    /// <summary>
    /// for unity configuration to determine which hand side is this
    /// </summary>
    public HandType handSide;

    /// <summary>
    /// the gender of the hand
    /// </summary>
    public GenderType HandGender;

    /// <summary>
    /// holds the strategy to handle hand movement for given experiment type
    /// </summary>
    private BaseExperimentStrategy mExperimentStrategy;

    /// <summary>
    /// The experiment type
    /// </summary>
    private ExperimentType mExperimentType;
    
    public bool IsCalibrating
    {
        get { return mExperimentStrategy.IsCalibrating; }
    }

    #endregion

    #region Functions
    void Start()
    {
        SetConfiguredHandAppearance();
        // are we in an animated hand?
        HandType handToAnimate = ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandToAnimate;
        if (!handToAnimate.Equals(handSide))        
            return; // we sholudn't animate this hand, so return and save your energy        
        
        // set parameters and init selected strategy
        mExperimentType = ConfigurationManager.Instance.Configuration.ExperimentType;
        if (CalibrationManager.Mode == HandPlayMode.Calibration)        
            mExperimentType = ExperimentType.Active;
        
        mExperimentStrategy = ExperimentStrategyFactory.GetOrCreate(mExperimentType);
        mExperimentStrategy.Init(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (mExperimentStrategy != null)
        {
            // only move the hand
            mExperimentStrategy.MoveHand();
        }
    }

    /// <summary>
    /// the function sets the appearance of the hand according to configuration
    /// </summary>
    public void SetConfiguredHandAppearance()
    {
        // get hand model
        var handModel = CommonUtilities.FindObjectWithName(transform, CommonUtilities.GetRendererParentObjectName(HandGender));
        // get renderer of hand 
        Renderer rend = handModel.GetComponent<Renderer>();
        // set hand color
        rend.material.SetColor(Shader.PropertyToID("_Tint"), ConfigurationManager.Instance.Configuration.VRHandConfiguration.HandColor);
    }

    private void OnDestroy()
    {
        if (mExperimentStrategy != null)
        {
            // when the hand is destroyed, close he strategy
            mExperimentStrategy.saveLogging();
            mExperimentStrategy.Close();
        }
    }

    public void recalibrateDevice()
    {
        mExperimentStrategy.recalibrate();
    }

    public void startLogging()
    {
        mExperimentStrategy.startLogging();
    }

    public void pauseLogging()
    {
        mExperimentStrategy.pauseLogging();
    }

    public void resumeLogging()
    {
        mExperimentStrategy.resumeLogging();
    }    

    #endregion
}