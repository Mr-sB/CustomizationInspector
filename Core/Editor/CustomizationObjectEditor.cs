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
			foldoutDrawer = new FoldoutDrawer(serializedObject, targets);
			buttonDrawer = new ButtonDrawer(serializedObject, targets);
			buttonDrawer.SetFoldoutDrawer(foldoutDrawer);
		}
		
		void OnDisable()
		{
			foldoutDrawer.SaveExpand();
		}

		public override void OnInspectorGUI()
		{
			//利用源码序列化
			EditorGUI.BeginChangeCheck();
			serializedObject.UpdateIfRequiredOrScript();
			SerializedProperty iterator = serializedObject.GetIterator();
			
			//m_Script property
			bool moveNextSuccess = iterator.NextVisible(true);
			if (moveNextSuccess)
				using (new EditorGUI.DisabledScope(true))
					EditorGUILayout.PropertyField(iterator, true);

			//Foldout
			foldoutDrawer.Draw();

			//Remain property
			if (moveNextSuccess)
			{
				while (iterator.NextVisible(false))
				{
					if (!foldoutDrawer.IsFoldout(iterator))
						EditorGUILayout.PropertyField(iterator, true);
				}
			}

			serializedObject.ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
			
			//利用base序列化
			// base.OnInspectorGUI();

			buttonDrawer.Draw();
		}
	}
}