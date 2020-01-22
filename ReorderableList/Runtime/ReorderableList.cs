using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Runtime {

	public abstract class ReorderableListBase{}
	
	public abstract class ReorderableList<T> : ReorderableListBase, ICloneable, IList<T>
	{
		[SerializeField]
		public List<T> List = new List<T>();

		public ReorderableList()
			: this(0) {
		}

		public ReorderableList(int length) {

			List = new List<T>(length);
		}

		public T this[int index] {

			get { return List[index]; }
			set { List[index] = value; }
		}
		
		public int Length {
			
			get { return List.Count; }
		}

		public bool IsReadOnly {

			get { return false; }
		}

		public int Count {

			get { return List.Count; }
		}

		public object Clone() {

			return new List<T>(List);
		}

		public void CopyFrom(IEnumerable<T> value) {

			List.Clear();
			List.AddRange(value);
		}

		public bool Contains(T value) {

			return List.Contains(value);
		}

		public int IndexOf(T value) {

			return List.IndexOf(value);
		}

		public void Insert(int index, T item) {

			List.Insert(index, item);
		}

		public void RemoveAt(int index) {

			List.RemoveAt(index);
		}

		public void Add(T item) {

			List.Add(item);
		}

		public void Clear() {

			List.Clear();
		}

		public void CopyTo(T[] array, int arrayIndex) {

			this.List.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {

			return List.Remove(item);
		}

		public T[] ToArray() {

			return List.ToArray();
		}

		public IEnumerator<T> GetEnumerator() {

			return List.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {

			return List.GetEnumerator();
		}

		public static implicit operator List<T>(ReorderableList<T> reorderableList)
		{
			return reorderableList.List;
		}
	}
}
