using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace CommonTools
{
    /// <summary>
    /// The class represents a configuration repository which is an XML file.
    /// impleemnts IConfigurationRepository accordingly.
    /// </summary>
    public class FileRepository : IConfigurationRepository
    {
        #region Data Members
        /// <summary>
        /// The path to the XML file.
        /// </summary>
        private string mPath;
        #endregion

        #region Functions
        /// <summary>
        /// refer to interface for documentation
        /// </summary>
        public bool Close()
        {
            // nothing to do here
            return true;
        }

        /// <summary>
        /// refer to interface for documentation
        /// </summary>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        public bool Connect(string connectionStr)
        {
            // save the path for later get or save functions
            mPath = connectionStr;
            return true;
        }

        /// <summary>
        /// refer to interface for documentation
        /// </summary>
        public T GetObject<T>()
        {
            // get serialized for the type
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T obj;

            // read the object
            obj = ReadFromFile<T>(serializer);
            return obj;
        }

        /// <summary>
        /// The function reads from file given in Connect function,
        /// Deserializes the xml file to an object of type T and returns deserialized object.
        /// throws Application exception on failure, and FileNotFound if opening the file failed.
        /// </summary>
        /// <typeparam name="T">the type of object that we want to get</typeparam>
        /// <param name="serializer"></param>
        /// <returns></returns>
        private T ReadFromFile<T>(XmlSerializer serializer)
        {
            T obj;
            try
            {
                FileStream readFS = new FileStream(mPath, FileMode.Open);
                obj = (T)serializer.Deserialize(readFS);
                readFS.Close();
                return obj;

            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }

        }

        /// <summary>
        /// refer to interface for documentation
        /// </summary>
        public object GetObject(Type objectType)
        {
            // get serialzied
            XmlSerializer serializer = new XmlSerializer(objectType);
            object obj;

            //read from file
            obj = ReadFromFile<object>(serializer);
            return obj;
        }

        /// <summary>
        /// refer to interface for documentation
        /// </summary>
        public bool Save(object obj, string path)
        {
            
            try
            {
                FileStream saveFileFS = new FileStream(path, FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(obj.GetType());

                serializer.Serialize(saveFileFS, obj);
                saveFileFS.Close();

            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Failed serializing to file : " + ex.Message);
                throw ex;
            }
            return true;
        } 
        #endregion
    }
}
