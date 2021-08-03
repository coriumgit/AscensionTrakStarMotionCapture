using System;

namespace CommonTools
{
    /// <summary>
    /// Attribute to apply to a connectino between GUI editor and configuration object.
    /// if assigend to a GUI type editor, given in constructor the type of object,
    /// if assigend to a configuratino object, given in constructor the type of GUI editor
    /// </summary>
    public class ConfigurationEditorAttribute : Attribute
    {
        #region Data Members
        // the type to associate with
        public Type ObjectType { get; private set; }
        #endregion

        #region Constructors
        public ConfigurationEditorAttribute(Type type)
        {
            ObjectType = type;
        } 
        #endregion
    }
}
