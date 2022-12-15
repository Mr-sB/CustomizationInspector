using System;
using System.Diagnostics;
using UnityEngine;
namespace CustomizationInspector.Runtime
{	
	public enum MessageType
	{
		None,
		Info,
		Warning,
		Error,
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class ButtonAttribute : Attribute
	{
		public readonly string ShowName;

		public ButtonAttribute(string showName = null)
		{
			ShowName = showName;
		}

		public override int GetHashCode()
		{
			return ShowName != null ? ShowName.GetHashCode() ^ base.GetHashCode() : base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class HideIfAttribute : PropertyAttribute
	{

		public readonly string MemberName;
		public readonly object[] Objs;
		public HideIfAttribute(string memberName, params object[] objs)
		{
			MemberName = memberName;
			Objs = objs;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class ShowIfAttribute : PropertyAttribute
	{
		public readonly string MemberName;
		public readonly object[] Objs;
		public ShowIfAttribute(string memberName, params object[] objs)
		{
			MemberName = memberName;
			Objs = objs;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class ReadOnlyAttribute : PropertyAttribute
	{ }

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class ValueDropdownAttribute : PropertyAttribute
	{

		public readonly string MemberName;
		public readonly object[] Objs;
		public ValueDropdownAttribute(string memberName, params object[] objs)
		{
			MemberName = memberName;
			Objs = objs;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	[Conditional("UNITY_EDITOR")]
	public class InfoBoxAttribute : PropertyAttribute
	{		
		public readonly string Description;
		public readonly MessageType MessageType;
		public readonly int Height;
		public InfoBoxAttribute(string description, MessageType messageType = MessageType.Info, int height = 24)
		{
			Description = description;
			MessageType = messageType;
			Height = height;
		}
	}
	
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class RenameAttribute : PropertyAttribute
	{
		public readonly string Rename;
		public RenameAttribute(string rename)
		{
			Rename = rename;
		}
	}
	
	/// <example>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[FoldoutAttribute("Foldout A")]
	/// 	public int intValue;
	/// 	[FoldoutAttribute("Foldout A/Sub Foldout")]
	/// 	public int intValue;
	/// 
	/// 	[FoldoutAttribute("Foldout A")]
	/// 	private void Jump()
	/// 	{
	/// 		// ...
	/// 	}
	/// 	[FoldoutAttribute("Foldout A/Sub Foldout")]
	/// 	private void Fly()
	/// 	{
	/// 		// ...
	/// 	}
	/// }
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class FoldoutAttribute : Attribute
	{
		public readonly string GroupName;
		public FoldoutAttribute(string groupName)
		{
			GroupName = groupName;
		}
	}
}