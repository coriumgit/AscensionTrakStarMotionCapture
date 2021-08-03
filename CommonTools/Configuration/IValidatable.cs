using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonTools
{
    /// <summary>
    /// interface for a validatable configuration object
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// The function that validates the object
        /// </summary>
        /// <returns>true for valdi object, false otherwise</returns>
        bool Validate();
    }

    /// <summary>
    /// generic class that validates given type. impelemnts IValidable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericValidator <T> : IValidatable
    {
        #region Data Members
        /// <summary>
        /// the object to validate
        /// </summary>
        T mValidatedObject;
        #endregion

        #region Ctors
        /// <summary>
        /// Initialized the object ot validate
        /// </summary>
        /// <param name="obj"></param>
        public GenericValidator(T obj)
        {
            mValidatedObject = obj;
        }
        #endregion

        #region Functions

        /// <summary>
        /// The function tries to validate the object, 
        /// and returns IEnumerable of properties in which we found invalid value
        /// </summary>
        /// <returns>IEnumerable of invalid properties. for valid object - empty one</returns>
        public IEnumerable<PropertyInfo> GetInvalidPropertiesErrors()
        {
            // get the properties
            var props = mValidatedObject.GetType().GetProperties();
            IList<PropertyInfo> invalidProperties = new List<PropertyInfo>();
            foreach (var prop in props)
            {
                // try to validate the property
                if (!ValidateProperty(prop))
                {
                    invalidProperties.Add(prop);
                }
            }
            return invalidProperties;
        }

        /// <summary>
        /// the function validates that reqiuered property is indeed included
        /// </summary>
        /// <param name="attributes">attributes of the property</param>
        /// <param name="value">value of the property</param>
        /// <returns>boolean indicating validity of requiered property aspect</returns>
        private static bool ValidateRequieredRule(IEnumerable<object> attributes, object value)
        {
            // get requiered attributes
            IList<ValidatorAttribute> validatorAttrs = new List<ValidatorAttribute>();
            foreach (var item in attributes)
            {
                validatorAttrs.Add(item as ValidatorAttribute);
            }
            // validate reqiured aspect
            var requiered = validatorAttrs.Where(x => x.IsRequiered == true);
            if (!requiered.Any())
            {
                return true;
            }
            if (string.IsNullOrEmpty(value.ToString()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// the function validates that reqiuered property is in given range of values
        /// <param name="attributes">attributes of the property</param>
        /// <param name="value">value of the property</param>
        /// <returns>boolean indicating validity of range of values in property aspect</returns>
        private static bool ValidateRangeRule(IEnumerable<object> attributes, object value)
        {
            // get range to check
            IList<ValidatorAttribute> validatorAttrs = new List<ValidatorAttribute>();
            foreach (var item in attributes)
            {
                validatorAttrs.Add(item as ValidatorAttribute);
            }
            var ranges = validatorAttrs.Where(x => (x.MinValue != double.MinValue || x.MaxValue != double.MaxValue));
            if (!ranges.Any())
            {
                return true;
            }

            // search for match of range
            bool isInSomeRange = false;
            foreach (var range in ranges)
            {
                // check is in range
                double doubleValue;
                if (!double.TryParse(value.ToString(), out doubleValue))
                {
                    return false;
                }
                if (range.MinValue <= doubleValue && range.MaxValue >= doubleValue)
                {
                    // in range
                    isInSomeRange = true;
                    break;
                }
            }

            return isInSomeRange;
        }

        /// <summary>
        /// the function validates that reqiuered property matches given regex expression
        /// </summary>
        /// <param name="attributes">attributes of the property</param>
        /// <param name="value">value of the property</param>
        /// <returns>boolean indicating validity of regex expression aspect</returns>
        private static bool ValidateRegexRule(IEnumerable<object> attributes, object value)
        {
            // get all validator attributes
            IList<ValidatorAttribute> validatorAttrs = new List<ValidatorAttribute>();
            foreach (var item in attributes)
            {
                validatorAttrs.Add(item as ValidatorAttribute);
            }
            // check validity of regex expression
            var regexExpressions = validatorAttrs.Where(x => (!string.IsNullOrEmpty(x.RegexRule)));
            if (!regexExpressions.Any())
            {
                return true;
            }
            // check of match of regex expression
            bool isRegexSuitable = true;
            foreach (var regexAttr in regexExpressions)
            {
                Regex regex = new Regex(regexAttr.RegexRule);
                isRegexSuitable &= regex.IsMatch(value.ToString());
            }
            return isRegexSuitable;
        }

        /// <summary>
        /// the function validates given property
        /// </summary>
        /// <param name="prop">the property to validate</param>
        /// <returns>boolean indicating validity</returns>
        public bool ValidateProperty(PropertyInfo prop)
        {
            return ValidateProperty(mValidatedObject, prop);
        }

        /// <summary>
        /// the fucntion validates the object's property.
        /// </summary>
        /// <param name="obj">the object that contains the property</param>
        /// <param name="prop">property to validate</param>
        /// <returns>boolean indicating validity</returns>
        public static bool ValidateProperty(Object obj, PropertyInfo prop)
        {
            // get properties
            var attributes = prop.GetCustomAttributes(typeof(ValidatorAttribute), true);
            if (attributes.Length == 0)
            {
                return true;
            }

            // check each aspect of validity
            var propValue = prop.GetValue(obj, null);
            if (!ValidateRequieredRule(attributes, propValue))
            {
                return false;
            }
            if (!ValidateRangeRule(attributes, propValue))
            {
                return false;
            }
            if (!ValidateRegexRule(attributes, propValue))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// the function checks if the object of the calss is valid
        /// </summary>
        /// <returns>boolean indicating validity</returns>
        bool IValidatable.Validate()
        {
            // there are invalid properties? then invalid, otherwise valid
            return (GetInvalidPropertiesErrors().Any() == false);
        } 
        #endregion
    }
}
