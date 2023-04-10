
#if UNITY_EDITOR


using System;
using System.Linq;
using System.Reflection;


namespace BrainFailProductions.PolyFew
{


    public class PrivateValueAccessor
    {
        public static BindingFlags Flags = BindingFlags.Instance
                                           | BindingFlags.GetProperty
                                           | BindingFlags.SetProperty
                                           | BindingFlags.GetField
                                           | BindingFlags.SetField
                                           | BindingFlags.NonPublic
                                           | BindingFlags.Public
                                           | BindingFlags.Static
                                           | BindingFlags.DeclaredOnly
                                           | BindingFlags.Default
                                           | BindingFlags.InvokeMethod;
        /// <summary>
        /// A static method to get the PropertyInfo of a private property of any object.
        /// </summary>
        /// <param name="type">The Type that has the private property</param>
        /// <param name="propertyName">The name of the private property</param>
        /// <returns>PropertyInfo object. It has the property name and a useful GetValue() method.</returns>
        public static PropertyInfo GetPrivatePropertyInfo(Type type, string propertyName)
        {
            var props = type.GetProperties(Flags);
            return props.FirstOrDefault(propInfo => propInfo.Name == propertyName);
        }

        /// <summary>
        /// A static method to get the value of a private property of any object.
        /// </summary>
        /// <param name="type">The Type that has the private property</param>
        /// <param name="propertyName">The name of the private property</param>
        /// <param name="o">The instance from which to read the private value.</param>
        /// <returns>The value of the property boxed as an object.</returns>
        public static object GetPrivatePropertyValue(Type type, string propertyName, object o)
        {
            return GetPrivatePropertyInfo(type, propertyName).GetValue(o, Flags, null, null, null);
        }

        /// <summary>
        /// A static method to get the FieldInfo of a private field of any object.
        /// </summary>
        /// <param name="type">The Type that has the private field</param>
        /// <param name="fieldName">The name of the private field</param>
        /// <returns>FieldInfo object. It has the field name and a useful GetValue() method.</returns>
        public static FieldInfo GetPrivateFieldInfo(Type type, string fieldName)
        {
            var fields = type.GetFields(Flags);
            return fields.FirstOrDefault(fieldInfo => fieldInfo.Name == fieldName);
        }

        /// <summary>
        /// A static method to get the FieldInfo of a private field of any object.
        /// </summary>
        /// <param name="type">The Type that has the private field</param>
        /// <param name="fieldName">The name of the private field</param>
        /// <param name="o">The instance from which to read the private value.</param>
        /// <returns>The value of the property boxed as an object.</returns>
        public static object GetPrivateFieldValue(Type type, string fieldName, object o)
        {
            return GetPrivateFieldInfo(type, fieldName).GetValue(o);
        }


    }

}


#endif