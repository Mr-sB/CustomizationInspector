using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	public class SurrogateTest : MonoBehaviour
	{

		[SerializeField] private MyClass[] objects;

		[SerializeField, ReorderableSetting(surrogateType = typeof(GameObject), surrogateProperty = "gameObject")]
		private MyClassArray myClassArray;

		[System.Serializable]
		public class MyClass
		{

			public string name;
			public GameObject gameObject;
		}

		[System.Serializable]
		public class MyClassArray : ReorderableList<MyClass>
		{
		}
	}
}
