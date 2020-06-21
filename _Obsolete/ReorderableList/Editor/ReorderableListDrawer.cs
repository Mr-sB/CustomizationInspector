//using System;
//using System.Reflection;
//using CustomizationInspector.Runtime;
//using UnityEditor;
//using UnityEditorInternal;
//using UnityEngine;
//
//namespace CustomizationInspector.Editor
//{
//	[CustomPropertyDrawer(typeof(ReorderableListBase), true)]
//	public class ReorderableListDrawer : PropertyDrawer
//	{
//		private ReorderableList mList; 
//		public static BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
//
//		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//		{
//			ReorderableList list = GetReorderableList(property);
//			list.DoList(position);
//		}
//
//		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//		{
//			return GetReorderableList(property).GetHeight();
//		}
//
//		private ReorderableList GetReorderableList(SerializedProperty property)
//		{
//			if (mList == null)
//			{
//				mList = InitReorderableList(property);
//			}
//			else
//			{
//				SerializedProperty listProperty = property.FindPropertyRelative("List");
//				mList.serializedProperty = listProperty;
//			}
//			return mList;
//		}
//
//		private ReorderableList InitReorderableList(SerializedProperty property)
//		{
//			SerializedProperty listProperty = property.FindPropertyRelative("List");
//			bool draggable, displayHeader, displayAddButton, displayRemoveButton;
//			//初始化设置
//			//优先使用字段上的Attribute值，其次使用类的值
//			ReorderableListDraggableAttribute[] draggableAttributes = fieldInfo.GetCustomAttributes(typeof(ReorderableListDraggableAttribute), true) as ReorderableListDraggableAttribute[];
//			if (draggableAttributes != null && draggableAttributes.Length > 0)
//				draggable = draggableAttributes[0].flag;
//			else
//				draggable = property.FindPropertyRelative("draggable").boolValue;
//
//			ReorderableListDisplayHeaderAttribute[] displayHeaderAttributes = fieldInfo.GetCustomAttributes(typeof(ReorderableListDisplayHeaderAttribute), true) as ReorderableListDisplayHeaderAttribute[];
//			if (displayHeaderAttributes != null && displayHeaderAttributes.Length > 0)
//				displayHeader = displayHeaderAttributes[0].flag;
//			else
//				displayHeader = property.FindPropertyRelative("displayHeader").boolValue;
//
//			ReorderableListDisplayAddButtonAttribute[] displayAddButtonAttributes = fieldInfo.GetCustomAttributes(typeof(ReorderableListDisplayAddButtonAttribute), true) as ReorderableListDisplayAddButtonAttribute[];
//			if (displayAddButtonAttributes != null && displayAddButtonAttributes.Length > 0)
//				displayAddButton = displayAddButtonAttributes[0].flag;
//			else
//				displayAddButton = property.FindPropertyRelative("displayAddButton").boolValue;
//
//			ReorderableListDisplayRemoveButtonAttribute[] displayRemoveButtonAttributes = fieldInfo.GetCustomAttributes(typeof(ReorderableListDisplayRemoveButtonAttribute), true) as ReorderableListDisplayRemoveButtonAttribute[];
//			if (displayRemoveButtonAttributes != null && displayRemoveButtonAttributes.Length > 0)
//				displayRemoveButton = displayRemoveButtonAttributes[0].flag;
//			else
//				displayRemoveButton = property.FindPropertyRelative("displayRemoveButton").boolValue;
//			//根据设置构造ReorderableList
//			ReorderableList _list = new ReorderableList(property.serializedObject, listProperty, draggable, displayHeader, displayAddButton, displayRemoveButton);
//
//			//获取到目前在绘制的SerializedProperty的object
//			object obj = property.GetObject();
//
//			Type type = fieldInfo.FieldType;
//			//反射委托
//			ReorderableListBase.HeaderCallbackDelegate drawHeaderCallback = type.GetField("drawHeaderCallback", bindingFlags).GetValue(obj) as ReorderableListBase.HeaderCallbackDelegate;
//			ReorderableListBase.FooterCallbackDelegate drawFooterCallback = type.GetField("drawFooterCallback", bindingFlags).GetValue(obj) as ReorderableListBase.FooterCallbackDelegate;
//			ReorderableListBase.ElementCallbackDelegate drawElementCallback = type.GetField("drawElementCallback", bindingFlags).GetValue(obj) as ReorderableListBase.ElementCallbackDelegate;
//			ReorderableListBase.ElementCallbackDelegate drawElementBackgroundCallback = type.GetField("drawElementBackgroundCallback", bindingFlags).GetValue(obj) as ReorderableListBase.ElementCallbackDelegate;
//			ReorderableListBase.ElementHeightCallbackDelegate elementHeightCallback = type.GetField("elementHeightCallback", bindingFlags).GetValue(obj) as ReorderableListBase.ElementHeightCallbackDelegate;
//			ReorderableListBase.ReorderCallbackDelegate onReorderCallback = type.GetField("onReorderCallback", bindingFlags).GetValue(obj) as ReorderableListBase.ReorderCallbackDelegate;
//			ReorderableListBase.SelectCallbackDelegate onSelectCallback = type.GetField("onSelectCallback", bindingFlags).GetValue(obj) as ReorderableListBase.SelectCallbackDelegate;
//			ReorderableListBase.AddCallbackDelegate onAddCallback = type.GetField("onAddCallback", bindingFlags).GetValue(obj) as ReorderableListBase.AddCallbackDelegate;
//			ReorderableListBase.AddDropdownCallbackDelegate onAddDropdownCallback = type.GetField("onAddDropdownCallback", bindingFlags).GetValue(obj) as ReorderableListBase.AddDropdownCallbackDelegate;
//			ReorderableListBase.RemoveCallbackDelegate onRemoveCallback = type.GetField("onRemoveCallback", bindingFlags).GetValue(obj) as ReorderableListBase.RemoveCallbackDelegate;
//			ReorderableListBase.SelectCallbackDelegate onMouseUpCallback = type.GetField("onMouseUpCallback", bindingFlags).GetValue(obj) as ReorderableListBase.SelectCallbackDelegate;
//			ReorderableListBase.CanRemoveCallbackDelegate onCanRemoveCallback = type.GetField("onCanRemoveCallback", bindingFlags).GetValue(obj) as ReorderableListBase.CanRemoveCallbackDelegate;
//			ReorderableListBase.CanAddCallbackDelegate onCanAddCallback = type.GetField("onCanAddCallback", bindingFlags).GetValue(obj) as ReorderableListBase.CanAddCallbackDelegate;
//			ReorderableListBase.ChangedCallbackDelegate onChangedCallback = type.GetField("onChangedCallback", bindingFlags).GetValue(obj) as ReorderableListBase.ChangedCallbackDelegate;
//
//			//添加回调
//			_list.drawHeaderCallback += (Rect rect) =>
//			{
//				if (drawHeaderCallback != null)
//					drawHeaderCallback(_list, rect);
//				else
//				//更改源码绘制为显示字段的名称
//				EditorGUI.LabelField(rect, property.displayName);
//				//ReorderableList.defaultBehaviours.DrawHeader(rect, property.serializedObject, _list.serializedProperty, _list.list);//源码绘制
//			};
//
//			_list.drawFooterCallback += (Rect rect) =>
//			{
//				if (drawFooterCallback != null)
//					drawFooterCallback(_list, rect);
//				else
//					ReorderableList.defaultBehaviours.DrawFooter(rect, _list);//源码绘制
//			};
//
//			_list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
//			{
//				if (drawElementCallback != null)
//					drawElementCallback(_list, rect, index, isActive, isFocused);
//				else
//				{
//					if (_list.serializedProperty != null)
//					{
//					//更改源码绘制为绘制property
//					SerializedProperty elementProperty = _list.serializedProperty.GetArrayElementAtIndex(index).Copy();
//						Rect elementRect = rect;
//						elementRect.y += 2;
//						elementProperty.NextVisible(true);//跳过List里的Element x描述
//					string elementPath = elementProperty.propertyPath.Substring(0, elementProperty.propertyPath.LastIndexOf('.'));
//						do
//						{
//							int idx = elementProperty.propertyPath.LastIndexOf('.');
//							if (idx <= 0 || elementPath != elementProperty.propertyPath.Substring(0, idx))
//								break;
//							EditorGUI.PropertyField(elementRect, elementProperty, true);
//							elementRect.y += 2 + EditorGUI.GetPropertyHeight(elementProperty, true);
//						} while (elementProperty.NextVisible(false));
//					}
//					else
//					{
//						ReorderableList.defaultBehaviours.DrawElement(rect, null, _list.list[index], isActive, isFocused, _list.draggable);
//					}
//
//					//源码绘制  绘制的是Label
//					//if (_list.serializedProperty != null)
//					//{
//					//	ReorderableList.defaultBehaviours.DrawElement(rect, _list.serializedProperty.GetArrayElementAtIndex(index), null, isActive, isFocused, _list.draggable);
//					//}
//					//else
//					//{
//					//	ReorderableList.defaultBehaviours.DrawElement(rect, null, _list.list[index], isActive, isFocused, _list.draggable);
//					//}
//				}
//			};
//
//			_list.drawElementBackgroundCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
//			{
//				if (drawElementBackgroundCallback != null)
//					drawElementBackgroundCallback(_list, rect, index, isActive, isFocused);
//				else
//					ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, _list.draggable);//源码绘制
//			};
//
//			_list.elementHeightCallback += (int index) =>
//			{
//				if (elementHeightCallback != null)
//					return elementHeightCallback(_list, index);
//				SerializedProperty elementProperty = _list.serializedProperty.GetArrayElementAtIndex(index).Copy();
//				float height = 2;
//				elementProperty.NextVisible(true);//跳过List里的Element x描述
//				string elementPath = elementProperty.propertyPath.Substring(0, elementProperty.propertyPath.LastIndexOf('.'));
//				do
//				{
//					int idx = elementProperty.propertyPath.LastIndexOf('.');
//					if (idx <= 0 || elementPath != elementProperty.propertyPath.Substring(0, idx))
//						break;
//					height += 2 + EditorGUI.GetPropertyHeight(elementProperty, true);
//				} while (elementProperty.NextVisible(false));
//				return height + 2;
//			//return _list.elementHeight;//源码默认返回的就是这个值
//			};
//
//			_list.onReorderCallback += (ReorderableList list) =>
//			{
//				if (onReorderCallback != null)
//					onReorderCallback(list);
//			};
//
//			_list.onSelectCallback += (ReorderableList list) =>
//			{
//				if (onSelectCallback != null)
//					onSelectCallback(list);
//			};
//
//			_list.onAddCallback += (ReorderableList list) =>
//			{
//				if (onAddCallback != null)
//					onAddCallback(list);
//				else
//					ReorderableList.defaultBehaviours.DoAddButton(list);//源码事件
//			};
//
//			_list.onAddDropdownCallback += (Rect buttonRect, ReorderableList list) =>
//			{
//				if (onAddDropdownCallback != null)
//					onAddDropdownCallback(buttonRect, list);
//				else
//					ReorderableList.defaultBehaviours.DoAddButton(list);//源码事件
//			};
//
//			_list.onRemoveCallback += (ReorderableList list) =>
//			{
//				if (onRemoveCallback != null)
//					onRemoveCallback(list);
//				else
//					ReorderableList.defaultBehaviours.DoRemoveButton(list);//源码事件
//			};
//
//			_list.onMouseUpCallback += (ReorderableList list) =>
//			{
//				if (onMouseUpCallback != null)
//					onMouseUpCallback(list);
//			};
//
//			_list.onCanRemoveCallback += (ReorderableList list) =>
//			{
//				if (onCanRemoveCallback != null)
//					return onCanRemoveCallback(list);
//				return true;//源码默认返回的就是这个值
//			};
//
//			_list.onCanAddCallback += (ReorderableList list) =>
//			{
//				if (onCanAddCallback != null)
//					return onCanAddCallback(list);
//				return true;//源码默认返回的就是这个值
//			};
//
//			_list.onChangedCallback += (ReorderableList list) =>
//			{
//				if (onChangedCallback != null)
//					onChangedCallback(list);
//			};
//
//			return _list;
//		}
//	}
//}