using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JasHandExperiment
{
    public interface IMinMaxCalculator <T> where T : IComparable
    {
        void SetInitialValues(T minDef, T maxDef);
        void InsertValue(T value);
        
        T Min { get;}
        T Max { get;}
    }

    class NaiveMinMax<T> : IMinMaxCalculator<T> where T : IComparable
    {
        T mCurrentMin;
        T mCurrentMax;
        public NaiveMinMax()
        {
        }

        public T Min
        {
            get
            {
                return mCurrentMin;
            }
        }

        public T Max
        {
            get
            {
                return mCurrentMax;
            }
        }
        public void InsertValue(T value)
        {
            if (value.CompareTo(mCurrentMax) > 0)
            {
                mCurrentMax = value;
            }
            if (value.CompareTo(mCurrentMin) < 0)
            {
                mCurrentMin = value;
            }
        }

        public void SetInitialValues(T minDef, T maxDef)
        {
            mCurrentMin = minDef;
            mCurrentMax = maxDef;
        }
    }


    public abstract class WeightedMinMax<T> : IMinMaxCalculator<T> where T : IComparable
    {
        private const int DICT_MAX_LEN = 2;

        Dictionary<T, uint> mMaxValsCounter;
        Dictionary<T, uint> mMinValsCounter;
        T mMinimalInMaxDict;
        T mMaximalInMinDict;
        T mDefaultMin;
        T mDefaultMax;

        public T Min
        {
            get
            {
                return CalcWeighted(mMinValsCounter);
            }
        }

        public T Max
        {
            get
            {
                return CalcWeighted(mMaxValsCounter);
            }
        }

        internal abstract T CalcWeighted(Dictionary<T, uint> vals);

        private void UpdateValuesDict(Dictionary<T,uint> dict, T value, ref T threshold)
        {
            if (dict.ContainsKey(value))
            {
                dict[value] = dict[value] + 1;
                return;
            }
            if (dict.Count < DICT_MAX_LEN)
            {
                dict.Add(value, 1);
                return;
            }
            dict.Remove(mMinimalInMaxDict);
            threshold = value;
            dict.Add(value, 1);
        }

        public void InsertValue(T value)
        {
            if (value.CompareTo(mMinimalInMaxDict) > 0)
            {
                UpdateValuesDict(mMaxValsCounter, value, ref mMinimalInMaxDict);
            }
            if (value.CompareTo(mMaximalInMinDict) < 0)
            {
                UpdateValuesDict(mMinValsCounter, value, ref mMaximalInMinDict);
            }
        }

        public void SetInitialValues(T minDef, T maxDef)
        {
            mMaxValsCounter = new Dictionary<T, uint>();
            mMinValsCounter = new Dictionary<T, uint>();
            mDefaultMax = maxDef;
            mDefaultMin = minDef;
            mMaxValsCounter.Add(mDefaultMax, 0);
            mMinimalInMaxDict = mDefaultMax;
            mMinValsCounter.Add(mDefaultMin, 0);
            mMaximalInMinDict = mDefaultMin;
        }
    }

    public class WeightedUShortMinMax : WeightedMinMax<ushort>
    {
        public WeightedUShortMinMax()
        {
            SetInitialValues(ushort.MaxValue, ushort.MinValue);
        }

        internal override ushort CalcWeighted(Dictionary<ushort, uint> vals)
        {
            long sum = 0;
            long amount = 0;
            foreach (var kvpVal in vals)
            {
                sum += kvpVal.Key * kvpVal.Value;
                amount += kvpVal.Value;
            }
            return (ushort)(sum / amount);
        }
    }


    public class WeightedFloatMinMax : WeightedMinMax<float>
    {
        internal override float CalcWeighted(Dictionary<float, uint> vals)
        {
            float sum = 0;
            long amount = 0;
            foreach (var kvpVal in vals)
            {
                sum += kvpVal.Key * kvpVal.Value;
                amount += kvpVal.Value;
            }
            return (sum / amount);
        }
    }
}
