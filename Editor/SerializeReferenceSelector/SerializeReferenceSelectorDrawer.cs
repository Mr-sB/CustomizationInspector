#if UNITY_2019_3_OR_NEWER
using System;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	[CustomPropertyDrawer(typeof(SerializeReferenceSelectorAttribute))]
	internal class SerializeReferenceSelectorDrawer : PropertyDrawer
	{
		private TypeDropdown typeDropdown;
		private SerializedProperty targetProperty;
		
		private TypeDropdown GetTypeDropdown(SerializedProperty property)
		{
			if (typeDropdown != null) return typeDropdown;
			Type baseType = property.GetManagedReferenceFieldType();
			var minLine = attribute is SerializeReferenceSelectorAttribute selectorAttribute ? selectorAttribute.MinLine : 16;
			typeDropdown = new TypeDropdown(baseType, minLine, new AdvancedDropdownState(), TypeDropdown.CustomClassValidation);
			typeDropdown.OnItemSelected += item => {
				Type type = item.Type;
				try
				{
					var oldValue = targetProperty.managedReferenceValue;
					var oldType = oldValue?.GetType();
					// Same type. Do nothing
					if (oldType == type) return;
					targetProperty.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
					targetProperty.isExpanded = true;
					// Save changes.
					targetProperty.serializedObject.ApplyModifiedProperties();
					targetProperty.serializedObject.Update();
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			};
			return typeDropdown;
		}
        
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				Rect dropdownRect = position;
				float xOffset = attribute is SerializeReferenceSelectorAttribute selectorAttribute && selectorAttribute.XOffset >= 0
					? selectorAttribute.XOffset
					: EditorGUIUtility.labelWidth;
				dropdownRect.xMin += xOffset;
				dropdownRect.height = EditorGUIUtility.singleLineHeight;
				
				var value = property.managedReferenceValue;
				string name = value?.GetType().FullName ?? TypeDropdown.NULL_TYPE_NAME;
				if (EditorGUI.DropdownButton(dropdownRect, EditorUtil.TempContent(name), FocusType.Keyboard))
				{
					targetProperty = property;
					GetTypeDropdown(property).Show(dropdownRect);
				}
			}

			EditorGUI.PropertyField(position, property, label, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}
	}
}
#endif
