#if UNITY_2019_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public class TypeDropdownItem : AdvancedDropdownItem
    {
        public readonly Type Type;

        public TypeDropdownItem(Type type, string name) : base(name)
        {
            Type = type;
        }
    }

    public class TypeDropdown : AdvancedDropdown
    {
        public const string GLOBAL_NAMESPACE = "<Global>";
        public const string NULL_TYPE_NAME = "<NULL>";
        public static readonly float HEADER_HEIGHT = EditorGUIUtility.singleLineHeight * 2f;
        
        private Func<Type, bool> validationFunc;
        private List<Type> showTypes = new List<Type>();
        
        public event Action<TypeDropdownItem> OnItemSelected;

        public TypeDropdown(Type baseType, int minLine, AdvancedDropdownState state, Func<Type, bool> validationFunc = null) : base(state)
        {
            this.validationFunc = validationFunc;
            SetTypes(new List<Type>{baseType}.Concat(TypeCache.GetTypesDerivedFrom(baseType)));
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * minLine + HEADER_HEIGHT);
        }
        
        public TypeDropdown(IEnumerable<Type> types, int minLine, AdvancedDropdownState state, Func<Type, bool> validationFunc = null) : base(state)
        {
            this.validationFunc = validationFunc;
            SetTypes(types);
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * minLine + HEADER_HEIGHT);
        }

        private void SetTypes(IEnumerable<Type> types)
        {
            showTypes.Clear();
            foreach (var type in types)
            {
                if (ValidateType(type))
                    showTypes.Add(type);
            }
        }
        
        private bool ValidateType(Type type)
        {
            if (type == null) return false;
            if (validationFunc != null)
                return validationFunc.Invoke(type);
            return true;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Type");
            InitRoot(root, showTypes);
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is TypeDropdownItem typeDropdownItem)
                OnItemSelected?.Invoke(typeDropdownItem);
        }
        
        private static void InitRoot(AdvancedDropdownItem root, IEnumerable<Type> types)
        {
            int itemCount = 0;

            // Add Null item first.
            root.AddChild(new TypeDropdownItem(null, NULL_TYPE_NAME)
            {
                id = itemCount++
            });
            
            string appearNamespace = null;
            bool hasDiffNamespace = false;
            bool hasGlobalType = false;
            foreach (Type type in types)
            {
                string ns = GetNamespace(type);
                if (!hasGlobalType && ns == GLOBAL_NAMESPACE)
                {
                    hasGlobalType = true;
                    if (hasDiffNamespace)
                        break;
                }
                if (!hasDiffNamespace)
                {
                    if (appearNamespace == null)
                        appearNamespace = ns;
                    else if (appearNamespace != ns)
                    {
                        hasDiffNamespace = true;
                        if (hasGlobalType)
                            break;
                    }
                }
            }

            // Add global second.
            if (hasGlobalType)
            {
                root.AddChild(new AdvancedDropdownItem(GLOBAL_NAMESPACE)
                {
                    id = itemCount++,
                });
            }

            // Add type items.
            foreach (Type type in types)
            {
                AdvancedDropdownItem parent = root;

                // Add namespace item.
                if (hasDiffNamespace)
                {
                    string ns = GetNamespace(type);
                    AdvancedDropdownItem foundItem = FindChild(parent, ns);
                    if (foundItem == null)
                    {
                        foundItem = new AdvancedDropdownItem(ns)
                        {
                            id = itemCount++,
                        };
                        parent.AddChild(foundItem);
                    }
                    parent = foundItem;
                }

                // Add type item.
                parent.AddChild(new TypeDropdownItem(type, ObjectNames.NicifyVariableName(type.Name))
                {
                    id = itemCount++
                });
            }
        }

        private static AdvancedDropdownItem FindChild(AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem item in parent.children)
                if (item.name == name)
                    return item;
            return null;
        }

        public static bool CustomClassValidation(Type type)
        {
            return !type.IsInterface && !type.IsAbstract && !type.IsGenericType
                   && Attribute.IsDefined(type, typeof(SerializableAttribute))
                   && !type.IsSubclassOf(typeof(UnityEngine.Object));
        }
        
        public static string GetNamespace(Type type)
        {
            if (type == null) return string.Empty;
            return string.IsNullOrEmpty(type.Namespace) ? GLOBAL_NAMESPACE : type.Namespace;
        }
    }
}
#endif
