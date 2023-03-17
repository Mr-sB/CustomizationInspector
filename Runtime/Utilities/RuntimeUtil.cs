using System;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    public static class RuntimeUtil
    {
        /// <summary>
        /// 根据类型返回该类型的默认值。值类型的返回值，引用类型返回null
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>默认值</returns>
        public static object GetDefaultForType(this Type type) 
        {
            if (type == null)
            {
                Debug.LogError("Type is null!");
                return null;
            }
            return type.IsValueType ? GetInstanceFromType(type) : null;
        }
        
        /// <summary>
        /// 根据类型返回该类型的默认值。值类型的返回值，引用类型返回null
        /// </summary>
        /// <param name="typeName">类型字符串(Type.AssemblyQualifiedName)</param>
        /// <returns>默认值</returns>
        public static object GetDefaultForType(string typeName) 
        {
            return GetDefaultForType(Type.GetType(typeName));
        }

        /// <summary>
        /// 根据类型返回该类型的实例值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>实例值</returns>
        public static object GetInstanceFromType(this Type type)
        {
            if (type == null)
            {
                Debug.LogError("Type is null!");
                return null;
            }

            if (type.IsAbstract)
            {
                Debug.LogError("Type is abstract!");
                return null;
            }

            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType(), 0);
            }
            return Activator.CreateInstance(type);
        }
        
        /// <summary>
        /// 根据类型返回该类型的实例值
        /// </summary>
        /// <param name="typeName">类型字符串(Type.AssemblyQualifiedName)</param>
        /// <returns>实例值</returns>
        public static object GetInstanceFromType(string typeName)
        {
            return GetInstanceFromType(Type.GetType(typeName));
        }
    }
}