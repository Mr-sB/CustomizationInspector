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
        
        public float? FixedLength { private set; get; }
        public bool IsFixed => FixedLength.HasValue;
        
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
        
        public static WindowNodeOption WithFixedLength(float? fixedLength)
        {
            return node =>
            {
                if (node == null) return;
                node.FixedLength = fixedLength;
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
            FixedLength = null;
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
        
        internal float CalcLength(float totalLength)
        {
            return IsFixed ? (FixedLength ?? 10) : (totalLength * CurLengthRatio);
        }

        public void Draw(Rect rect)
        {
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
