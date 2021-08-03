using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTools
{
    /// <summary>
    /// Attribute to decorate a property of a configuration object.
    /// multiple attributes will apply complex validation patterns
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ValidatorAttribute : Attribute
    {
        /// <summary>
        /// is the property required to be set?
        /// </summary>
        public bool IsRequiered { get; set; }

        /// <summary>
        /// the maximal value of the property
        /// </summary>
        public double  MaxValue { get; set; }

        /// <summary>
        /// the minimal value of the property
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Regex rule for string validation
        /// </summary>
        public string RegexRule { get; set; }

        public ValidatorAttribute()
        {
            IsRequiered = false;
            MaxValue = double.MaxValue;
            MinValue = double.MinValue;
            RegexRule = string.Empty;
        }
    }
}
