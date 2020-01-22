using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	[CreateAssetMenu(fileName = "New ScriptableObject Example", menuName = "ScriptableObject Example")]
	public class ScriptableObjectExample : ScriptableObject
	{

		[SerializeField, ReorderableSetting(paginate = true, pageSize = 0, elementNameProperty = "myString")]
		private MyList list;

		[System.Serializable]
		private struct MyObject
		{

			public bool myBool;
			public float myValue;
			public string myString;

			public MyObject(bool myBool, float myValue, string myString)
			{

				this.myBool = myBool;
				this.myValue = myValue;
				this.myString = myString;
			}
		}

		[System.Serializable]
		private class MyList : ReorderableList<MyObject>
		{
		}
	}
}
