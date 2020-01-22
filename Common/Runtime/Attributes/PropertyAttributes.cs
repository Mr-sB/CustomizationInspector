using System;
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

	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class ButtonAttribute : Attribute
	{

		public readonly string ShowName;
		public ButtonAttribute(string showName)
		{
			ShowName = showName;
		}
		public ButtonAttribute()
		{
			ShowName = null;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
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

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
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

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public class ReadOnlyAttribute : PropertyAttribute
	{ }

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
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
	
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public class RenameAttribute : PropertyAttribute
	{
		public readonly string Rename;
		public RenameAttribute(string rename)
		{
			Rename = rename;
		}
	}
}