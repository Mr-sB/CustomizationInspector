using UnityEngine;
using CustomizationInspector.Runtime;

namespace CustomizationInspector.Example
{
	public class GameObjectExample : MonoBehaviour
	{

		//There's a bug with Unity and rendering when an Object has no CustomEditor defined. As in this example
		//The list will reorder correctly, but depth sorting and animation will not update :(
		[ReorderableSetting(paginate = true, pageSize = 2)]
		public GameObjectList list;

		[System.Serializable]
		public class GameObjectList : ReorderableList<GameObject>
		{
		}

		private void Update()
		{

			if (Input.GetKeyDown(KeyCode.Space))
			{

				list.Add(gameObject);
			}
		}
	}
}
