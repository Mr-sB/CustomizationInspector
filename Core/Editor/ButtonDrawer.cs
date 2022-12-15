using System;
using System.Collections.Generic;
using System.Reflection;
using CustomizationInspector.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomizationInspector.Editor
{
    public class ButtonDrawer : Drawer
    {
	    private class Parameter
	    {
		    private GUIContent content;
		    public readonly string Name;
		    public readonly Type Type;
		    public object Value { private set; get; }
		    
		    public Parameter(string name, Type type, object value)
		    {
			    Name = name;
			    Type = type;
			    Value = value;
			    content = new GUIContent(Name);
			    if (value != null)
			    {
				    var valueType = value.GetType();
				    //Change type to value's type
				    if (Type != valueType && Type.IsAssignableFrom(valueType))
					    Type = valueType;
			    }
		    }

		    public Parameter(ParameterInfo parameterInfo, object value) : this(parameterInfo.Name, parameterInfo.ParameterType, value)
		    {
		    }

		    public void Draw(Object target)
		    {
			    Value = FieldInspector.DrawFieldLayout(content, Type, Value, target);
		    }
	    }
	    
	    private class Button
	    {
		    public static readonly GUIStyle HeaderStyle = "RL Header";
		    public const int InvokeButtonWidth = 50;
		    public const int InvokeButtonHeight = 19;
		    public const int InvokeButtonSpace = 2;
		    
		    public readonly MethodInfo MethodInfo;
		    public readonly Parameter[] Parameters;
		    public readonly string Name;
		    public readonly GUIContent Content;
		    public bool Expand => expand ?? true;
		    private bool? expand;

		    public Button(MethodInfo methodInfo, string name)
		    {
			    MethodInfo = methodInfo;
			    Name = name ?? methodInfo.Name;
			    var parameterInfos = methodInfo.GetParameters();
			    if (parameterInfos.Length == 0)
			    {
				    Parameters = null;
				    Content = new GUIContent(name ?? methodInfo.Name);
			    }
			    else
			    {
				    Parameters = new Parameter[parameterInfos.Length];
				    Content = new GUIContent("Invoke");
				    for (var i = 0; i < parameterInfos.Length; i++)
				    {
					    var parameterInfo = parameterInfos[i];
					    object value = null;
					    bool hasValue = false;
					    if (parameterInfo.HasDefaultValue)
					    {
						    //Use default value
						    hasValue = true;
						    value = parameterInfo.DefaultValue;
					    }

					    if (!hasValue)
					    {
						    var defaultValueAttribute = parameterInfo.GetCustomAttribute<System.ComponentModel.DefaultValueAttribute>();
						    if (defaultValueAttribute != null)
						    {
							    //Use default value
							    hasValue = true;
							    value = defaultValueAttribute.Value;
						    }
					    }

					    if (!hasValue)
					    {
						    //Create default
						    value = parameterInfo.ParameterType.GetDefaultForType();
					    }
					    Parameters[i] = new Parameter(parameterInfo, value);
				    }
			    }
		    }

		    public void Draw(Object[] targets)
		    {
			    void InvokeMethod()
			    {
				    foreach (var target in targets)
				    {
					    try
					    {
						    object[] objs = null;
						    if (Parameters != null)
						    {
							    objs = new object[Parameters.Length];
							    for (var i = 0; i < Parameters.Length; i++)
							    {
								    var parameter = Parameters[i];
								    objs[i] = parameter.Value;
							    }
						    }
						    MethodInfo.Invoke(target, objs);
					    }
					    catch (Exception e)
					    {
						    Debug.LogError(e, target);
					    }
				    }
			    }
			
			    if (Parameters == null)
			    {
				    Rect position = GUILayoutUtility.GetRect(Content, GUI.skin.button);
				    position = EditorGUI.IndentedRect(position);
				    if (GUI.Button(position, Content))
					    InvokeMethod();
			    }
			    else
			    {
					Rect position = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, EditorGUIUtility.fieldWidth, InvokeButtonHeight,
					    InvokeButtonHeight, EditorStyles.foldout);
					if (Event.current.type == EventType.Repaint)
					{
						EditorGUI.indentLevel--;
						Rect headerPosition = EditorGUI.IndentedRect(position);
						EditorGUI.indentLevel++;
						HeaderStyle.Draw(headerPosition, false, false, false, false);
					}
					Rect foldoutPosition = new Rect(position);
				    foldoutPosition.width -= InvokeButtonWidth + InvokeButtonSpace;
				    if (!expand.HasValue)
					    expand = FieldInspector.LoadFoldoutExpand(FieldInspector.GetFoldoutSaveKey(Name, targets[0]));
				    expand = FieldInspector.DrawFoldout(foldoutPosition, expand.Value, Name, targets[0], true);
				    Rect buttonPosition = new Rect(position);
				    buttonPosition.xMin = buttonPosition.xMax - InvokeButtonWidth;
				    if (GUI.Button(buttonPosition, Content))
					    InvokeMethod();

				    if (expand.Value)
				    {
					    EditorGUI.indentLevel++;
					    foreach (var parameter in Parameters)
						    parameter.Draw(targets[0]);
					    EditorGUI.indentLevel--;
				    }
				    
				    // Rect lastRect = GUILayoutUtility.GetLastRect();
				    // Rect backgroundPosition = new Rect(position);
				    // backgroundPosition.y += backgroundPosition.height;
				    // backgroundPosition.yMax = lastRect.yMax;
				    // if (Event.current.type == EventType.Repaint)
					   //  ((GUIStyle) "RL Background").Draw(backgroundPosition, false, false, false, false);
			    }
		    }
	    }

	    private static readonly GUIContent tmpContent = new GUIContent();
	    
	    private MethodInfo[] methodInfos;
	    private Dictionary<MethodInfo, List<ButtonAttribute>> methodButtonAttributes;
	    private Dictionary<ButtonAttribute, Button> buttonCache;
		private FoldoutDrawer foldoutDrawer;
	    
		private static GUIContent TempContent(string label)
		{
			tmpContent.image = null;
			tmpContent.text = label;
			tmpContent.tooltip = null;
			return tmpContent;
		}
		
	    public ButtonDrawer(SerializedObject serializedObject, Object[] targets) : base(serializedObject, targets)
	    {
	        methodInfos = targetType.GetMethods(BindingFlags);
	        methodButtonAttributes = new Dictionary<MethodInfo, List<ButtonAttribute>>(methodInfos.Length);
	        buttonCache = new Dictionary<ButtonAttribute, Button>();
	    }

	    public void SetFoldoutDrawer(FoldoutDrawer drawer)
	    {
		    foldoutDrawer = drawer;
			drawer?.SetDrawMethodHandler(DrawButtons);
	    }
        
	    public override void Draw()
	    {
		    foreach (var methodInfo in methodInfos)
		    {
			    if (foldoutDrawer != null && foldoutDrawer.IsFoldout(methodInfo)) continue;
			    DrawButtons(methodInfo, targets);
		    }
	    }

	    public void DrawButtons(MethodInfo methodInfo, Object[] targets)
		{
			if (!methodButtonAttributes.TryGetValue(methodInfo, out var buttonAttributes))
			{
				object[] attributes = methodInfo.GetCustomAttributes(true);
				buttonAttributes = new List<ButtonAttribute>();
				methodButtonAttributes.Add(methodInfo, buttonAttributes);
				foreach (object attribute in attributes)
				{
					if (attribute is ButtonAttribute buttonAttribute)
						buttonAttributes.Add(buttonAttribute);
				}
			}
			foreach (var buttonAttribute in buttonAttributes)
				DrawButton(methodInfo, buttonAttribute, targets);
		}

		public void DrawButton(MethodInfo methodInfo, ButtonAttribute buttonAttribute, Object[] targets)
		{
			if (!buttonCache.TryGetValue(buttonAttribute, out var button))
			{
				button = new Button(methodInfo, buttonAttribute.ShowName);
				buttonCache.Add(buttonAttribute, button);
			}
			button.Draw(targets);
		}
		
		public void SaveExpand()
		{
			foreach (var pair in buttonCache)
				foreach (var obj in targets)
					FieldInspector.SaveFoldoutExpand(FieldInspector.GetFoldoutSaveKey(pair.Value.Name, obj), pair.Value.Expand);
		}
    }
}