using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Editor {

	[CustomPropertyDrawer(typeof(ReorderableListBase), true)]
	public class ReorderableListDrawer : PropertyDrawer {

		public const string LIST_PROPERTY_NAME = "List";

		private static Dictionary<int, ReorderableList> mLists = new Dictionary<int, ReorderableList>();

		public override bool CanCacheInspectorGUI(SerializedProperty property) {

			return false;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			ReorderableList list = GetList(property, GetReorderableAttribute());

			return list != null ? list.GetHeight() : EditorGUIUtility.singleLineHeight;
		}		

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			ReorderableList list = GetList(property, GetReorderableAttribute());

			if (list != null) {

				list.DoList(EditorGUI.IndentedRect(position), label);
			}
			else
			{
				Debug.LogError("List must extend from ReorderableList");
				GUI.Label(position, "List must extend from ReorderableList", EditorStyles.label);
			}
		}

		public static int GetListId(SerializedProperty property) {

			if (property != null) {

				int h1 = property.serializedObject.targetObject.GetHashCode();
				int h2 = property.propertyPath.GetHashCode();

				return (((h1 << 5) + h1) ^ h2);
			}

			return 0;
		}

		public static ReorderableList GetList(SerializedProperty property) {

			return GetList(property, null, GetListId(property));
		}

		public static ReorderableList GetList(SerializedProperty property, ReorderableSettingAttribute attrib) {

			return GetList(property, attrib, GetListId(property));
		}

		public static ReorderableList GetList(SerializedProperty property, int id) {

			return GetList(property, null, id);
		}

		public static ReorderableList GetList(SerializedProperty property, ReorderableSettingAttribute attrib, int id) {

			if (property == null) {

				return null;
			}

			ReorderableList reorderableList = null;
			SerializedProperty list = property.FindPropertyRelative(LIST_PROPERTY_NAME);

			if (list != null && list.isArray) {

				if (!mLists.TryGetValue(id, out reorderableList)) {

					if (attrib != null) {

						Texture icon = !string.IsNullOrEmpty(attrib.elementIconPath) ? AssetDatabase.GetCachedIcon(attrib.elementIconPath) : null;

						ReorderableList.ElementDisplayType displayType = attrib.singleLine ? ReorderableList.ElementDisplayType.SingleLine : ReorderableList.ElementDisplayType.Auto;

						reorderableList = new ReorderableList(list, attrib.add, attrib.remove, attrib.draggable, displayType, attrib.elementNameProperty, attrib.elementNameOverride, icon);
						reorderableList.paginate = attrib.paginate;
						reorderableList.pageSize = attrib.pageSize;
						reorderableList.sortable = attrib.sortable;
						reorderableList.elementLabels = attrib.labels;

						//handle surrogate if any

						if (attrib.surrogateType != null) {

							SurrogateCallback callback = new SurrogateCallback(attrib.surrogateProperty);

							reorderableList.surrogate = new ReorderableList.Surrogate(attrib.surrogateType, callback.SetReference);
						}
					}
					else {

						reorderableList = new ReorderableList(list, true, true, true);
					}

					mLists.Add(id, reorderableList);
				}
				else {

					reorderableList.List = list;
				}
			}

			return reorderableList;
		}

		private ReorderableSettingAttribute GetReorderableAttribute()
		{
			ReorderableSettingAttribute[] reorderableSettingAttributes = fieldInfo.GetCustomAttributes(typeof(ReorderableSettingAttribute), true) as ReorderableSettingAttribute[];
			if (reorderableSettingAttributes != null && reorderableSettingAttributes.Length > 0) return reorderableSettingAttributes[0];
			return null;
		}

		private struct SurrogateCallback {

			private string property;

			internal SurrogateCallback(string property) {

				this.property = property;
			}

			internal void SetReference(SerializedProperty element, Object objectReference, ReorderableList list) {

				SerializedProperty prop = !string.IsNullOrEmpty(property) ? element.FindPropertyRelative(property) : null;

				if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference) {

					prop.objectReferenceValue = objectReference;
				}
			}
		}
	}
}
