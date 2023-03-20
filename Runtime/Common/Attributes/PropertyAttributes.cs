using System;
using System.Diagnostics;
using System.IO;
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
	
	public enum PathLocation
	{
		CustomFolder,
		ProjectFolder,
		AssetsFolder,
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
	public class HideIfAttribute : ShowIfAttribute
	{
		public HideIfAttribute(string memberName, params object[] objs) : base(memberName, objs)
		{
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
	
	[Conditional("UNITY_EDITOR")]
	public abstract class PathAttribute : PropertyAttribute
	{
		public readonly PathLocation Location;
		public readonly string RootFolder;
		public abstract bool IsFile { get; }
		public bool Browse = true;
		public bool Open = true;
		public bool Draggable = true;
		
		public PathAttribute(PathLocation location = PathLocation.ProjectFolder, string rootFolder = null)
		{
			Location = location;
			switch (location)
			{
				case PathLocation.CustomFolder:
					RootFolder = rootFolder;
					break;
				case PathLocation.ProjectFolder:
					RootFolder = Environment.CurrentDirectory;
					break;
				case PathLocation.AssetsFolder:
					RootFolder = Application.dataPath;
					break;
			}
		}
		
		public PathAttribute(string rootFolder) : this(PathLocation.CustomFolder, rootFolder)
		{
		}
		
		public string GetRelativePath(string path)
		{
			if (string.IsNullOrEmpty(RootFolder) || string.IsNullOrEmpty(path)) return path;
			return Path.GetRelativePath(RootFolder, path);
		}

		public string GetAbsolutePath(string relativePath)
		{
			if (string.IsNullOrEmpty(RootFolder)) return relativePath;
			return Path.GetFullPath(Path.Combine(RootFolder, relativePath ?? ""));
		}
	}
	
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class FilePathAttribute : PathAttribute
	{
		public override bool IsFile => true;

		public FilePathAttribute(PathLocation location = PathLocation.ProjectFolder, string rootFolder = null) : base(location, rootFolder)
		{
		}
		
		public FilePathAttribute(string rootFolder) : base(rootFolder)
		{
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class FolderPathAttribute : PathAttribute
	{
		public override bool IsFile => false;
		
		public FolderPathAttribute(PathLocation location = PathLocation.ProjectFolder, string rootFolder = null) : base(location, rootFolder)
		{
		}
		
		public FolderPathAttribute(string rootFolder) : base(rootFolder)
		{
		}
	}
}
