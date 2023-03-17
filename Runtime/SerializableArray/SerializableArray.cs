using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Runtime
{
    [Serializable]
    public class SerializableArray<T> : IList<T>
    {
        [SerializeField] private List<T> list;
        public List<T> List => list;

        public SerializableArray()
        {
            list = new List<T>();
        }

        public SerializableArray(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public SerializableArray(int capacity)
        {
            list = new List<T>(capacity);
        }

        public T this[int index]
        {
            get { return list[index]; }

            set { list[index] = value; }
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public static implicit operator List<T>(SerializableArray<T> array)
        {
            return array.list;
        }
    }

    #region BasicType

    [Serializable]
    public class SerializableIntArray : SerializableArray<int>
    {

    }

    [Serializable]
    public class SerializableIntArray2D : SerializableArray<SerializableIntArray>
    {

    }

    [Serializable]
    public class SerializableIntArray3D : SerializableArray<SerializableIntArray2D>
    {

    }

    [Serializable]
    public class SerializableFloatArray : SerializableArray<float>
    {

    }

    [Serializable]
    public class SerializableFloatArray2D : SerializableArray<SerializableFloatArray>
    {

    }

    [Serializable]
    public class SerializableFloatArray3D : SerializableArray<SerializableFloatArray2D>
    {

    }

    [Serializable]
    public class SerializableStringArray : SerializableArray<string>
    {

    }

    [Serializable]
    public class SerializableStringArray2D : SerializableArray<SerializableStringArray>
    {

    }

    [Serializable]
    public class SerializableStringArray3D : SerializableArray<SerializableStringArray2D>
    {

    }

    #endregion

    #region UnityType

    [Serializable]
    public class SerializableGameObjectArray : SerializableArray<GameObject>
    {

    }

    [Serializable]
    public class SerializableGameObjectArray2D : SerializableArray<SerializableGameObjectArray>
    {

    }

    [Serializable]
    public class SerializableGameObjectArray3D : SerializableArray<SerializableGameObjectArray2D>
    {

    }

    [Serializable]
    public class SerializableTransformArray : SerializableArray<Transform>
    {

    }

    [Serializable]
    public class SerializableTransformArray2D : SerializableArray<SerializableTransformArray>
    {

    }

    [Serializable]
    public class SerializableTransformArray3D : SerializableArray<SerializableTransformArray2D>
    {

    }

    [Serializable]
    public class SerializableVector2Array : SerializableArray<Vector2>
    {

    }

    [Serializable]
    public class SerializableVector2Array2D : SerializableArray<SerializableVector2Array>
    {

    }

    [Serializable]
    public class SerializableVector2Array3D : SerializableArray<SerializableVector2Array2D>
    {

    }

    [Serializable]
    public class SerializableVector3Array : SerializableArray<Vector3>
    {

    }

    [Serializable]
    public class SerializableVector3Array2D : SerializableArray<SerializableVector3Array>
    {

    }

    [Serializable]
    public class SerializableVector3Array3D : SerializableArray<SerializableVector3Array2D>
    {

    }

    [Serializable]
    public class SerializableVector4Array : SerializableArray<Vector4>
    {

    }

    [Serializable]
    public class SerializableVector4Array2D : SerializableArray<SerializableVector4Array>
    {

    }

    [Serializable]
    public class SerializableVector4Array3D : SerializableArray<SerializableVector4Array2D>
    {

    }

    [Serializable]
    public class SerializableColorArray : SerializableArray<Color>
    {

    }

    [Serializable]
    public class SerializableColorArray2D : SerializableArray<SerializableColorArray>
    {

    }

    [Serializable]
    public class SerializableColorArray3D : SerializableArray<SerializableColorArray2D>
    {

    }

    #endregion
}