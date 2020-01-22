using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
	//直接读取对应的PropertyAttribute，进行相应的绘制
	[CustomPropertyDrawer(typeof(InfoBoxAttribute))]
	internal class InfoBoxDrawer : DecoratorDrawer
	{
		InfoBoxAttribute infoBox;
		public override void OnGUI(Rect position)
		{
			if (infoBox == null)
				infoBox = attribute as InfoBoxAttribute;
			position = EditorGUI.IndentedRect(position);
			EditorGUI.HelpBox(position, infoBox.Description, (UnityEditor.MessageType) infoBox.MessageType);
		}

		public override float GetHeight()
		{
			return (attribute as InfoBoxAttribute).Height;
		}
	}
}
