using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTools
{
    public static class StringUtilities
    {

        /// <summary>
        /// helper function that convers floats to strings for given line
        /// </summary>
        /// <param name="floatLine">float array to be converted into string array (a line to be in a CSV File)</param>
        /// <returns>string array, each element is a parse of a float in original floatLine</returns>
        public static string[] FloatToStringArray(float[] floatLine)
        {
            string[] strArr = new string[floatLine.Length];
            for (int i = 0; i < floatLine.Length; ++i)
            {
                strArr[i] = floatLine[i].ToString();
            }
            return strArr;
        }

        public static float[] StringArrayToFloatArray(string[] scaledSensors)
        {
            float[] scaledSensorsFloat = new float[scaledSensors.Length];
            for (int i = 0; i < scaledSensors.Length; i++)
            {
                if (!float.TryParse(scaledSensors[i], out scaledSensorsFloat[i]))
                {
                    scaledSensorsFloat[i] = 0;
                    UnityEngine.Debug.Log("float not parsed well => " + scaledSensors[i]);
                }
            }

            return scaledSensorsFloat;
        }
    }
}
