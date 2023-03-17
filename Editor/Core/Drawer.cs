using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
    public abstract class Drawer
    {
        public static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.NonPublic |
                                                           BindingFlags.Public | BindingFlags.Static;
        
        protected SerializedObject serializedObject;
        protected Object target;
        protected Object[] targets;
        protected Type targetType;
        
        public Drawer(SerializedObject serializedObject, Object target, Object[] targets)
        {
            this.serializedObject = serializedObject;
            this.target = target;
            this.targets = targets;
            targetType = target.GetType();
        }

        public abstract void Draw();
    }
}