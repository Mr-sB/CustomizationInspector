using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomizationInspector.Editor
{
	public static class EditorUtil
	{
		#region TempGUIContent

        private static readonly GUIContent textContent = new GUIContent();
        private static readonly GUIContent imageContent = new GUIContent();
        private static readonly GUIContent textImageContent = new GUIContent();
        
        public static GUIContent TempContent(string t)
        {
            return TempContent(t, string.Empty);
        }

        public static GUIContent TempContent(string t, string tooltip)
        {
            textContent.text = t;
            textContent.tooltip = tooltip;
            return textContent;
        }

        public static GUIContent TempContent(Texture i)
        {
            return TempContent(i, string.Empty);
        }

        public static GUIContent TempContent(Texture i, string tooltip)
        {
            imageContent.image = i;
            imageContent.tooltip = tooltip;
            return imageContent;
        }

        public static GUIContent TempContent(string t, Texture i)
        {
            return TempContent(t, i, string.Empty);
        }
        
        public static GUIContent TempContent(string t, Texture i, string tooltip)
        {
            textImageContent.text = t;
            textImageContent.image = i;
            textImageContent.tooltip = tooltip;
            return textImageContent;
        }
        
        public static GUIContent TempContent(GUIContent other)
        {
            other ??= GUIContent.none;
            return TempContent(other.text, other.image, other.tooltip);
        }
        
        public static GUIContent TempImageOrTextContent(string t, Texture i, string tooltip = "")
        {
            if (i)
            {
                textImageContent.text = string.Empty;
                textImageContent.image = i;
            }
            else
            {
                textImageContent.text = t;
                textImageContent.image = null;
            }
            textImageContent.tooltip = tooltip;
            return textImageContent;
        }
        
        public static GUIContent TempImageOrTextContent(GUIContent other)
        {
            other ??= GUIContent.none;
            return TempImageOrTextContent(other.text, other.image, other.tooltip);
        }

        #endregion
		
		private static Regex camelCaseRegex =
			new Regex("(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])",
				RegexOptions.IgnorePatternWhitespace);

		private static Dictionary<string, string> camelCaseSplit = new Dictionary<string, string>();

		[NonSerialized]
		private static Dictionary<Type, Dictionary<FieldInfo, bool>> attributeFieldCache =
			new Dictionary<Type, Dictionary<FieldInfo, bool>>();

		public static string SplitCamelCase(string s)
		{
			if (s.Equals(string.Empty))
			{
				return s;
			}

			if (camelCaseSplit.ContainsKey(s))
			{
				return camelCaseSplit[s];
			}

			string key = s;
			s = s.Replace("_uScript", "uScript");
			s = s.Replace("_PlayMaker", "PlayMaker");
			if (s.Length > 2 && s.StartsWith("m_"))
			{
				s = s.Substring(2);
			}
			else if (s.Length > 1 && s[0].CompareTo('_') == 0)
			{
				s = s.Substring(1);
			}

			s = camelCaseRegex.Replace(s, " ");
			s = s.Replace("_", " ");
			s = s.Replace("u Script", " uScript");
			s = s.Replace("Play Maker", "PlayMaker");
			s = (char.ToUpper(s[0]) + s.Substring(1)).Trim();
			camelCaseSplit.Add(key, s);
			return s;
		}

		public static bool HasAttribute(FieldInfo field, Type attributeType)
		{
			Dictionary<FieldInfo, bool> dictionary = null;
			if (attributeFieldCache.ContainsKey(attributeType))
			{
				dictionary = attributeFieldCache[attributeType];
			}

			if (dictionary == null)
			{
				dictionary = new Dictionary<FieldInfo, bool>();
			}

			if (dictionary.ContainsKey(field))
			{
				return dictionary[field];
			}

			bool flag = field.GetCustomAttributes(attributeType, false).Length > 0;
			dictionary.Add(field, flag);
			if (!attributeFieldCache.ContainsKey(attributeType))
			{
				attributeFieldCache.Add(attributeType, dictionary);
			}

			return flag;
		}
		
		private static byte[] ReadToEnd(Stream stream)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = stream.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, count);
				}

				result = memoryStream.ToArray();
			}

			return result;
		}

		public static float RoundToNearest(float num, float baseNum)
		{
			return (float) ((int) Math.Round((double) (num / baseNum), MidpointRounding.AwayFromZero)) * baseNum;
		}

		public static void SetObjectDirty(UnityEngine.Object obj)
		{
			if (EditorApplication.isPlaying)
			{
				return;
			}

			EditorUtility.SetDirty(obj);
			if (!EditorUtility.IsPersistent(obj))
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				return;
			}
		}
		
		public static bool IsEditPrefab()
		{
			return TryGetCurrentPrefabStage(out var prefabStage);
		}

		public static bool TryGetCurrentPrefabStage(out PrefabStage prefabStage)
		{
			prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
			return prefabStage != null;
		}
	}
}

