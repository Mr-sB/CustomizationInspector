using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	public class NestedExample : MonoBehaviour
	{

		[ReorderableSetting] public ExampleChildList list;

		[System.Serializable]
		public class ExampleChild
		{

			[ReorderableSetting(singleLine = true)] public NestedChildList nested;
		}

		[System.Serializable]
		public class NestedChild
		{

			public float myValue;
		}

		[System.Serializable]
		public class NestedChildCustomDrawer
		{

			public bool myBool;
			public GameObject myGameObject;
		}

		[System.Serializable]
		public class ExampleChildList : ReorderableList<ExampleChild>
		{
		}

		[System.Serializable]
		public class NestedChildList : ReorderableList<NestedChildCustomDrawer>
		{
		}
	}
}
