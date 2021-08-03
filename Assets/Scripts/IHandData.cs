using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JasHandExperiment
{
    /// <summary>
    /// interface for data structure regarding hand movement
    /// </summary>
    public interface IHandData : ICloneable
    {
        /// <summary>
        /// The function sets the abstract movement data to the instance of IHandData
        /// </summary>
        /// <param name="data">abstract data to set inner state from</param>/// <param name="data"></param>
        void SetHandMovementData(object data);
    }
}
