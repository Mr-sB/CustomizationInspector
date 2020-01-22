using UnityEngine;
using System.Collections.Generic;

using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	public class Example : MonoBehaviour
	{

		public List<ExampleChild> list1;

		[ReorderableSetting] public ExampleChildList list2;

		[ReorderableSetting] public ExampleChildList list3;

		[ReorderableSetting] public StringList list4;

		[ReorderableSetting] public VectorList list5;

		[System.Serializable]
		public class ExampleChild
		{

			public string name;
			public float value;
			public ExampleEnum myEnum;
			public LayerMask layerMask;
			public long longValue;
			public char charValue;
			public byte byteValue;

			public enum ExampleEnum
			{
				EnumValue1,
				EnumValue2,
				EnumValue3
			}
		}

		[System.Serializable]
		public class ExampleChildList : ReorderableList<ExampleChild>
		{
		}

		[System.Serializable]
		public class StringList : ReorderableList<string>
		{
		}

		[System.Serializable]
		public class VectorList : ReorderableList<Vector3>
		{
		}
	}
}
