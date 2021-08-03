using System;

namespace CommonTools
{
    /// <summary>
    /// The interface for a repository to save configuration to
    /// </summary>
    public interface IConfigurationRepository
    {
        /// <summary>
        /// the functino initialized connection with the repository
        /// </summary>
        /// <param name="connectionStr">parameter to find repository and connect to it</param>
        /// <returns>boolean indicating successful operation</returns>
        bool Connect(string connectionStr);

        /// <summary>
        /// the function saves the object to repository at given path
        /// </summary>
        /// <param name="obj">object to save</param>
        /// <param name="path">the path where to save</param>
        /// <returns>boolean indicating successful operation</returns>
        bool Save(Object obj, string path);

        /// <summary>
        /// The function retrieves the object from repository
        /// </summary>
        /// <typeparam name="T"> the type of object to retrieve</typeparam>
        /// <returns>the object from repository</returns>
        T GetObject<T>();

        /// <summary>
        /// The function retrieves the object from repository
        /// </summary>
        /// <param name="objectType">the type of the object to retrieve</param>
        /// <returns>the object from repository</returns>
        object GetObject(Type objectType);

        /// <summary>
        /// the function to be called when we finished usign the repository
        /// </summary>
        /// <returns>boolean indicating successful operation</returns>
        bool Close();
    }
}
