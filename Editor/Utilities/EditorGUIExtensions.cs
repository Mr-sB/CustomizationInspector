using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public static class EditorGUIExtensions
    {
        /// <summary>Creates a rect that can be grabbed and pulled</summary>
        /// <param name="rect">The draggable rect.</param>
        /// <param name="cursor">The cursor.</param>
        /// <returns>The mouse delta position.</returns>
        public static Vector2 SlideRect(Rect rect, MouseCursor? cursor = null)
        {
            if (!GUI.enabled)
                return Vector2.zero;
            if (cursor.HasValue)
                EditorGUIUtility.AddCursorRect(rect, cursor.Value);
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            if (GUI.enabled && Event.current.type == UnityEngine.EventType.MouseDown &&
                (Event.current.button == 0 && rect.Contains(Event.current.mousePosition)))
            {
                GUIUtility.hotControl = controlId;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();
            }
            else if (GUIUtility.hotControl == controlId)
            {
                if (Event.current.type == UnityEngine.EventType.MouseDrag)
                {
                    Event.current.Use();
                    GUI.changed = true;
                    return Event.current.delta;
                }

                if (Event.current.type == UnityEngine.EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }

            return Vector2.zero;
        }

        /// <summary>Creates a rect that can be grabbed and pulled</summary>
        /// <param name="defaultPosition">Default position.</param>
        /// <param name="rect">The draggable rect.</param>
        /// <param name="cursor">The cursor.</param>
        /// <returns>The mouse delta of given rect.</returns>
        public static Vector2 SlideRect(Vector2 defaultPosition, Rect rect, MouseCursor? cursor = null)
        {
            if (!GUI.enabled)
                return defaultPosition;
            if (cursor.HasValue)
                EditorGUIUtility.AddCursorRect(rect, cursor.Value);
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            if (GUI.enabled && Event.current.type == UnityEngine.EventType.MouseDown &&
                (Event.current.button == 0 && rect.Contains(Event.current.mousePosition)))
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();
                return Event.current.mousePosition - rect.position;
            }

            if (GUIUtility.hotControl == controlId)
            {
                if (Event.current.type == UnityEngine.EventType.MouseDrag)
                {
                    GUI.changed = true;
                    Event.current.Use();
                    return Event.current.mousePosition - rect.position;
                }

                if (Event.current.type == UnityEngine.EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }

            return defaultPosition;
        }
        
        public static bool DrawDefaultInspector(SerializedObject obj)
        {
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true);
            }
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }
        
        public static bool DrawDefaultInspectorWithoutScript(SerializedObject obj)
        {
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if ("m_Script" != iterator.propertyPath)
                    EditorGUILayout.PropertyField(iterator, true);
            }
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }
        
        public static bool DrawDefaultInspectorExcluding(SerializedObject obj, params string[] propertyToExclude)
        {
            EditorGUI.BeginChangeCheck();
            obj.UpdateIfRequiredOrScript();
            SerializedProperty iterator = obj.GetIterator();
            for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
            {
                if (!propertyToExclude.Contains(iterator.propertyPath))
                    EditorGUILayout.PropertyField(iterator, true);
            }
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }

        // private static readonly Func<Rect, Rect> unclipDelegate = (Func<Rect, Rect>)Delegate.CreateDelegate(typeof(Func<Rect, Rect>), typeof(GUI).Assembly.GetType("UnityEngine.GUIClip")
        //     .GetMethod("Unclip", new Type[] { typeof(Rect) }));
        //
        // public static Rect Unclip(Rect rect)
        // {
        //     return unclipDelegate(rect);
        // }
    }
}
