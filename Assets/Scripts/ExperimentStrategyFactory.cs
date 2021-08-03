using System;
using CommonTools;
using JasHandExperiment.Configuration;

namespace JasHandExperiment
{
    /// <summary>
    /// Factory class for ExperimentStrategies, creates only one copy of each strategy.
    /// </summary>
    public static class ExperimentStrategyFactory
    {
        #region Data Members
        /// <summary>
        /// member of Active experiment strategy
        /// </summary>
        private static ActiveExperimentStrategy sActiveStrategy;
        
        /// <summary>
        /// member of Simulationexperiment strategy
        /// </summary>
        private static PassiveSimulationExperimentStrategy sSimulationStrategy;
        
        /// <summary>
        /// member of Replay experiment strategy
        /// </summary>
        private static PassiveReplayExperimentStrategy sReplayStrategy;
        #endregion

        #region Functions
        /// <summary>
        /// The function gets relevant instance of experiment strategy,
        /// if the strategy wasn't already created it will be created otherwise just returned.
        /// </summary>
        /// <param name="exType">ExperimentType that the strategy belongs to</param>
        /// <returns>abstraction of an experiment strategy according to exType</returns>
        static public BaseExperimentStrategy GetOrCreate(ExperimentType exType)
        {
            BaseExperimentStrategy strategy = null;

            switch (exType)
            {
                case ExperimentType.Active:
                    if (sActiveStrategy == null)
                    {
                        sActiveStrategy = new ActiveExperimentStrategy();
                    }
                    strategy = sActiveStrategy;
                    break;
                case ExperimentType.PassiveSimulation:
                    if (sSimulationStrategy == null)
                    {
                        sSimulationStrategy = new PassiveSimulationExperimentStrategy();
                    }
                    strategy = sSimulationStrategy;
                    break;
                case ExperimentType.PassiveWatchingReplay:
                    if (sReplayStrategy == null)
                    {
                        sReplayStrategy = new PassiveReplayExperimentStrategy();
                    }
                    strategy = sReplayStrategy;
                    break;
                default:
                    break;
            }

            return strategy;
        }
        #endregion
    }
}