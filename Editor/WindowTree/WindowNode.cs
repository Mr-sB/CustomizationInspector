using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public abstract class WindowNode
    {
        public delegate void WindowNodeOption(WindowNode node);

        public string Name;
        public ContainerNode Parent { internal set; get; }

        public ContainerNode Root
        {
            get
            {
                if (Parent == null) return null;
                var root = Parent;
                while (root.Parent != null)
                    root = root.Parent;
                return root;
            }
        }
        
        public virtual WindowTree Owner => Root?.Owner;
        
        public Rect Position { private set; get; }
        
        private float minLengthRatio;
        public float MinLengthRatio
        {
            set => minLengthRatio = Mathf.Clamp01(value);
            get => minLengthRatio;
        }
        
        public float? FixedWidth { private set; get; }
        public float? FixedHeight { private set; get; }
        
        public float CurLengthRatio { internal set; get; }

        public bool Replaceable { private set; get; }
        
        // Temp cache variables
        internal int? resizeDragTargetNodeIndex;
        internal bool resizable => resizeDragTargetNodeIndex.HasValue;
        
        public static WindowNodeOption WithMinLengthRatio(float minLengthRatio)
        {
            return node =>
            {
                if (node == null) return;
                node.MinLengthRatio = minLengthRatio;
            };
        }
        
        public static WindowNodeOption WithFixedWidth(float? fixedWidth)
        {
            return node =>
            {
                if (node == null) return;
                node.FixedWidth = fixedWidth;
            };
        }
        
        public static WindowNodeOption WithFixedHeight(float? fixedHeight)
        {
            return node =>
            {
                if (node == null) return;
                node.FixedHeight = fixedHeight;
            };
        }
        
        public static WindowNodeOption WithCurLengthRatio(float curLengthRatio)
        {
            return node =>
            {
                if (node == null) return;
                node.CurLengthRatio = curLengthRatio;
            };
        }
        
        public static WindowNodeOption WithReplaceable(bool replaceable)
        {
            return node =>
            {
                if (node == null) return;
                node.Replaceable = replaceable;
            };
        }
        
        public WindowNode(string name, params WindowNodeOption[] options) : this(name)
        {
            InitOptions(options);
        }
        
        protected WindowNode(string name)
        {
            Name = name;
            MinLengthRatio = 0.1f;
            FixedWidth = null;
            FixedHeight = null;
            Replaceable = true;
        }
        
        protected void InitOptions(params WindowNodeOption[] options)
        {
            if (options != null && options.Length > 0)
            {
                foreach (var option in options)
                    option?.Invoke(this);
            }
        }

        public bool IsFixed()
        {
            if (Parent == null) return false;
            return IsFixed(Parent.Direction == ContainerNode.LayoutDirection.Horizontal);
        }
        
        public bool IsFixed(bool horizontal)
        {
            return GetFixedLength(horizontal).HasValue;
        }

        public float? GetFixedLength()
        {
            if (Parent == null) return null;
            return GetFixedLength(Parent.Direction == ContainerNode.LayoutDirection.Horizontal);
        }
        
        public float? GetFixedLength(bool horizontal)
        {
            return horizontal ? FixedWidth : FixedHeight;
        }
        
        internal float CalcLength(bool horizontal, float totalLength)
        {
            float? fixedLength = GetFixedLength(horizontal);
            return fixedLength ?? (totalLength * CurLengthRatio);
        }

        public void Draw(Rect rect)
        {
            if (FixedWidth.HasValue)
                rect.width = Mathf.Min(rect.width, FixedWidth.Value);
            if (FixedHeight.HasValue)
                rect.height = Mathf.Min(rect.width, FixedHeight.Value);
            Position = rect;
            DoDraw(rect);
        }

        protected virtual void DoDraw(Rect rect)
        {
            // Draw self GUI
            OnGUI(rect);
        }

        protected abstract void OnGUI(Rect rect);
        
        public virtual void Query(Rect queryRect, [NotNull] HashSet<WindowNode> results)
        {
            if (Position.Overlaps(queryRect))
                results.Add(this);
        }
        
        /// <summary>
        /// Call this method to enable replace function 
        /// </summary>
        /// <param name="dragRect"></param>
        /// <returns></returns>
        protected void DrawReplaceDraggable(Rect dragRect)
        {
            if (!Replaceable) return;
            EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);
            
            if (GUI.enabled && Event.current.type == UnityEngine.EventType.MouseDown &&
                (Event.current.button == 0 && dragRect.Contains(Event.current.mousePosition)))
                Owner.StartReplaceDrag(this);
        }
    }
}
