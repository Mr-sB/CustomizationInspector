using UnityEditor;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
	[CustomEditor(typeof(Object), true)]
	[CanEditMultipleObjects]
	public class CustomizationObjectEditor : UnityEditor.Editor
	{
		private FoldoutDrawer foldoutDrawer;
		private ButtonDrawer buttonDrawer;

		private void OnEnable()
		{
			if (!target)
				return;
			foldoutDrawer = new FoldoutDrawer(serializedObject, target, targets);
			buttonDrawer = new ButtonDrawer(serializedObject, target, targets);
			buttonDrawer.SetFoldoutDrawer(foldoutDrawer);
		}
		
		void OnDisable()
		{
			foldoutDrawer?.SaveExpand();
			buttonDrawer?.SaveExpand();
		}

		public override void OnInspectorGUI()
		{
			if (!target)
			{
				base.OnInspectorGUI();
				return;
			}
			//利用源码序列化
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			
			foldoutDrawer.BeginManualDraw();
			for (bool enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false)
			{
				using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
				{
					if (!foldoutDrawer.IsDrawn(iterator) && !foldoutDrawer.DrawFoldout(iterator))
						EditorGUILayout.PropertyField(iterator, true);
				}
			}

			foldoutDrawer.DrawRemainFoldout();

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
			
			//利用base序列化
			// base.OnInspectorGUI();

			buttonDrawer.Draw();
		}
	}
}