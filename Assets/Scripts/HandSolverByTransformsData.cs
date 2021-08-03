using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace JasHandExperiment
{
    class HandSolverByTransformsData : HandSolverBase
    {
        private IEnumerator<TransformsData> transformsDataEnumerator;        

        public HandSolverByTransformsData(Transform handJoint, string transformsDataFullPath) : base(handJoint) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(transformsDataFullPath, FileMode.Open);
            List<TransformsData> t = new List<TransformsData>(bf.Deserialize(file) as List<TransformsData>);
            transformsDataEnumerator = loadNextTransformsData(t.GetEnumerator());            
            file.Close();
        }

        public override void resolve()
        {
            if (transformsDataEnumerator.MoveNext())
                loadTransformsData(transformsDataEnumerator.Current);                        
        }

        private IEnumerator<TransformsData> loadNextTransformsData(List<TransformsData>.Enumerator transformsDataEnumerator)
        {
            bool hasNext = transformsDataEnumerator.MoveNext();
            DateTime resolutionStartTime = DateTime.Now;
            while (hasNext)
            {
                TransformsData transformsData = transformsDataEnumerator.Current;
                while (DateTime.Now - resolutionStartTime < transformsData.dataTimeStamp)
                    yield return transformsData;

                hasNext = transformsDataEnumerator.MoveNext();
            }
        }
    }
}
