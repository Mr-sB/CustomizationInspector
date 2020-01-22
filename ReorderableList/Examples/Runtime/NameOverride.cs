using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	public class NameOverride : MonoBehaviour
	{

		public string nameOverride = "Car";
		public string nestedNameOverride = "Car Part";

		[ReorderableSetting(null, "Car", null)] public ExampleChildList autoNameList;

		[ReorderableSetting] public DynamicExampleChildList dynamicNameList;

		[System.Serializable]
		public class ExampleChild
		{

			[ReorderableSetting(null, "Car Part", null)] public StringList nested;
		}

		[System.Serializable]
		public class DynamicExampleChild
		{

			[ReorderableSetting] public StringList nested;
		}

		[System.Serializable]
		public class ExampleChildList : ReorderableList<ExampleChild>
		{
		}

		[System.Serializable]
		public class DynamicExampleChildList : ReorderableList<DynamicExampleChild>
		{
		}

		[System.Serializable]
		public class StringList : ReorderableList<string>
		{
		}
	}
}
