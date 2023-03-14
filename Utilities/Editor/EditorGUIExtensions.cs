using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public static class EditorGUIExtensions
    {
        /// <summary>Creates a rect that can be grabbed and pulled</summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="cursor">The cursor.</param>
        /// <returns>The the mouse delta position.</returns>
        public static Vector2 SlideRect(Rect rect, MouseCursor cursor = MouseCursor.SlideArrow)
        {
            if (!GUI.enabled)
                return Vector2.zero;
            EditorGUIUtility.AddCursorRect(rect, cursor);
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
        /// <param name="position">The position.</param>
        /// <param name="rect">The grabbable rect.</param>
        /// <returns>The the mouse delta position.</returns>
        public static Vector2 SlideRect(Vector2 position, Rect rect)
        {
            if (!GUI.enabled)
                return position;
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
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

            return position;
        }
    }
}