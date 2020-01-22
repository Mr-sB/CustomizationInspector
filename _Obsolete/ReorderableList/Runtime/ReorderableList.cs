//using System.Collections.Generic;
//using UnityEditorInternal;
//using UnityEngine;
//
//namespace CustomizationInspector.Runtime
//{
//	[System.Serializable]
//	public abstract class ReorderableListBase
//	{
//		[SerializeField]
//		private bool draggable = true;
//		[SerializeField]
//		private bool displayHeader = true;
//		[SerializeField]
//		private bool displayAddButton = true;
//		[SerializeField]
//		private bool displayRemoveButton = true;
//
//		public delegate void FooterCallbackDelegate(ReorderableList list, Rect rect);//add
//		public delegate void ElementCallbackDelegate(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused);//add
//		public delegate bool CanAddCallbackDelegate(ReorderableList list);
//		public delegate void ReorderCallbackDelegate(ReorderableList list);
//		public delegate void SelectCallbackDelegate(ReorderableList list);
//		public delegate void AddCallbackDelegate(ReorderableList list);
//		public delegate void AddDropdownCallbackDelegate(Rect buttonRect, ReorderableList list);
//		public delegate void RemoveCallbackDelegate(ReorderableList list);
//		public delegate void ChangedCallbackDelegate(ReorderableList list);
//		public delegate bool CanRemoveCallbackDelegate(ReorderableList list);
//		public delegate void HeaderCallbackDelegate(ReorderableList list, Rect rect);//add
//		public delegate float ElementHeightCallbackDelegate(ReorderableList list, int index);//add
//
//		public HeaderCallbackDelegate drawHeaderCallback;
//		public FooterCallbackDelegate drawFooterCallback;
//		public ElementCallbackDelegate drawElementCallback;
//		public ElementCallbackDelegate drawElementBackgroundCallback;
//		public ElementHeightCallbackDelegate elementHeightCallback;
//		public ReorderCallbackDelegate onReorderCallback;
//		public SelectCallbackDelegate onSelectCallback;
//		public AddCallbackDelegate onAddCallback;
//		public AddDropdownCallbackDelegate onAddDropdownCallback;
//		public RemoveCallbackDelegate onRemoveCallback;
//		public SelectCallbackDelegate onMouseUpCallback;
//		public CanRemoveCallbackDelegate onCanRemoveCallback;
//		public CanAddCallbackDelegate onCanAddCallback;
//		public ChangedCallbackDelegate onChangedCallback;
//
//		public ReorderableListBase(bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton)
//		{
//			this.draggable = draggable;
//			this.displayHeader = displayHeader;
//			this.displayAddButton = displayAddButton;
//			this.displayRemoveButton = displayRemoveButton;
//		}
//
//		//private void DrawHeaderCallback(ReorderableList list, Rect rect)
//		//{
//		//	if (drawHeaderCallback != null)
//		//		drawHeaderCallback(list, rect);
//		//}
//
//		//private void DrawFooterCallback(ReorderableList list, Rect rect)
//		//{
//		//	if (drawFooterCallback != null)
//		//		drawFooterCallback(list, rect);
//		//}
//
//		//private void DrawElementCallback(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
//		//{
//		//	if (drawElementCallback != null)
//		//		drawElementCallback(list, rect, index, isActive, isFocused);
//		//}
//
//		//private void DrawElementBackgroundCallback(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
//		//{
//		//	if (drawElementBackgroundCallback != null)
//		//		drawElementBackgroundCallback(list, rect, index, isActive, isFocused);
//		//}
//
//		//private void ElementHeightCallback(ReorderableList list, int index)
//		//{
//		//	if (elementHeightCallback != null)
//		//		elementHeightCallback(list, index);
//		//}
//
//		//private void OnReorderCallback(ReorderableList list)
//		//{
//		//	if (onReorderCallback != null)
//		//		onReorderCallback(list);
//		//}
//
//		//private void OnSelectCallback(ReorderableList list)
//		//{
//		//	if (onSelectCallback != null)
//		//		onSelectCallback(list);
//		//}
//
//		//private void OnAddCallback(ReorderableList list)
//		//{
//		//	if (onAddCallback != null)
//		//		onAddCallback(list);
//		//}
//
//		//private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
//		//{
//		//	if (onAddDropdownCallback != null)
//		//		onAddDropdownCallback(buttonRect, list);
//		//}
//
//		//private void OnRemoveCallback(ReorderableList list)
//		//{
//		//	if (onRemoveCallback != null)
//		//		onRemoveCallback(list);
//		//}
//
//		//private void OnMouseUpCallback(ReorderableList list)
//		//{
//		//	if (onMouseUpCallback != null)
//		//		onMouseUpCallback(list);
//		//}
//
//		//private void OnCanRemoveCallback(ReorderableList list)
//		//{
//		//	if (onCanRemoveCallback != null)
//		//		onCanRemoveCallback(list);
//		//}
//
//		//private void OnCanAddCallback(ReorderableList list)
//		//{
//		//	if (onCanAddCallback != null)
//		//		onCanAddCallback(list);
//		//}
//
//		//private void OnChangedCallback(ReorderableList list)
//		//{
//		//	if (onChangedCallback != null)
//		//		onChangedCallback(list);
//		//}
//	}
//
//	/*
//	对于ReorderableList<T>，如果T是class
//	那么其中的List的某一项List[i]不要进行长时间引用！
//	在Reorder之后，其实并不是交换顺序，而是把相应序号的对象值进行交换
//	也就是说引用的对象的值发生了变化！
//	*/
//	[System.Serializable]
//	public abstract class ReorderableList<T> : ReorderableListBase
//	{
//		public List<T> List = new List<T>();
//
//		public ReorderableList(bool draggable = true, bool displayHeader = true, bool displayAddButton = true, bool displayRemoveButton = true)
//			: base(draggable, displayHeader, displayAddButton, displayRemoveButton)
//		{
//		}
//
//		public static implicit operator List<T>(ReorderableList<T> value)
//		{
//			return value.List;
//		}
//	}
//}