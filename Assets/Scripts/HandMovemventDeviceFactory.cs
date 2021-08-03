using System;
using System.Collections.Generic;
using CommonTools;
using JasHandExperiment.Configuration;
using UnityEngine;

namespace JasHandExperiment
{
    /// <summary>
    /// Factory class for HandMovementDevices, creates only one copy of each Device.
    /// </summary>
    public static class HandMovemventDeviceFactory
    {
        #region Data Members
        /// <summary>
        /// dictionary that holds all of the devices
        /// </summary>
        private static Dictionary<Type, IHandMovementDevice> sTypesDict;
        #endregion

        static HandMovemventDeviceFactory()
        {
            sTypesDict = new Dictionary<Type, IHandMovementDevice>();
            sTypesDict.Add(typeof(GlovesDevice), null);
            sTypesDict.Add(typeof(KeyBoardSimulationFileDevice), null);
            sTypesDict.Add(typeof(SimulationFileDevice), null);
            sTypesDict.Add(typeof(ReplayFileDevice), null);
        }

        #region Functions

        /// <summary>
        /// The function gets relevant instance of hand movement device,
        /// if the device wasn't already created it will be created otherwise just returned.
        /// </summary>
        /// <param name="exType">ExperimentType that the device belongs to</param>
        /// <returns>abstraction of an device according to exType</returns>
        static public IHandMovementDevice GetOrCreate(ExperimentType exType)
        {
            IHandMovementDevice device = null;

            switch (exType)
            {
                case ExperimentType.Active:
                    device = GetOrCreate<GlovesDevice>();
                    break;
                case ExperimentType.PassiveSimulation:
                    device = GetOrCreate<SimulationFileDevice>();
                    break;
                case ExperimentType.PassiveWatchingReplay:
                    device = GetOrCreate<ReplayFileDevice>();
                    break;
                default:
                    break;
            }

            return device;
        }

        /// <summary>
        /// The function gets relevant instance of hand movement device,
        /// if the device wasn't already created it will be created otherwise just returned.
        /// </summary>
        /// <param name="T">the type of device to get</param>
        /// <returns>abstraction of a device according to type of device</returns>
        static public T GetOrCreate<T>() where T : class,  IHandMovementDevice
        {
            Type devType = typeof(T);
            if (!sTypesDict.ContainsKey(devType))
            {
                Debug.Log("factory cannot create this type : " + devType.AssemblyQualifiedName);
            }

            T device = sTypesDict[devType] as T;
            if (device == null)
            {
                device = Activator.CreateInstance(devType) as T;
                sTypesDict[devType] = device;
            }

            return device;
        }


        #endregion
    }
}