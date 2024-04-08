using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public abstract class WindowNode : IEnumerable<WindowNode>
    {
        public enum LayoutDirection
        {
            Horizontal,
            Vertical,
        }

        public delegate void WindowNodeOption(WindowNode node);
        
        public const float DRAGGABLE_SPACE = 4;

        public string Name;
        public WindowNode Parent { private set; get; }
        public readonly List<WindowNode> Children;
        
        public LayoutDirection Direction;

        private float minLengthRatio;
        public float MinLengthRatio
        {
            set => minLengthRatio = Mathf.Clamp01(value);
            get => minLengthRatio;
        }
        
        public float? FixedLength { private set; get; }
        public bool IsFixed => FixedLength.HasValue;

        public float CurLengthRatio { private set; get; }

        // Temp cache variables
        private int? dragTargetNodeIndex;
        private bool draggable => dragTargetNodeIndex.HasValue;

        public static WindowNodeOption WithDirection(LayoutDirection direction)
        {
            return node =>
            {
                if (node == null) return;
                node.Direction = direction;
            };
        }
        
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
        
        public WindowNode(string name, params WindowNodeOption[] options)
        {
            Name = name;
            Children = new List<WindowNode>();
            Direction = LayoutDirection.Horizontal;
            MinLengthRatio = 0.1f;
            FixedLength = null;
            if (options != null && options.Length > 0)
            {
                foreach (var option in options)
                    option?.Invoke(this);
            }
        }

        public virtual void Draw(Rect rect)
        {
            // Draw self GUI
            OnGUI(rect);
            
            // Calc gui rect
            float totalMinRatio = 0;
            float totalFixedLength = 0;
            float totalDraggableLength = 0;
            int flexibleCount = 0;
            int? preFlexibleNodeIndex = null;
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                node.dragTargetNodeIndex = null;
                if (node.IsFixed)
                {
                    // Between two flexible node
                    if (preFlexibleNodeIndex.HasValue && FindNextFlexibleNode(i + 1) != null)
                    {
                        node.dragTargetNodeIndex = preFlexibleNodeIndex; // Fixed node, drag previous flexible node
                        totalDraggableLength += DRAGGABLE_SPACE;
                    }
                    
                    totalFixedLength += node.FixedLength.Value;
                }
                else
                {
                    var nextFlexibleNode = FindNextFlexibleNode(i + 1);
                    // In front of flexible node
                    if (nextFlexibleNode != null)
                    {
                        node.dragTargetNodeIndex = i; // Flexible node, drag itself
                        totalDraggableLength += DRAGGABLE_SPACE;
                    }
                    
                    preFlexibleNodeIndex = i;
                    flexibleCount++;
                    totalMinRatio += node.MinLengthRatio;
                }
            }

            float minRatioScale = 1;
            float curRatioScale = 1;
            float initScale = 1 / totalMinRatio;
            if (totalMinRatio > 1)
                minRatioScale = initScale; // Adjust
            
            // Init flexible size
            float usedRatio = 0;
            int flexibleIndex = 0;
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (node.IsFixed) continue;
                // Adjust length ratio
                float lengthRatio = node.MinLengthRatio * minRatioScale;
                if (flexibleIndex < flexibleCount - 1)
                {
                    if (node.CurLengthRatio <= 0)
                        node.CurLengthRatio = node.MinLengthRatio * initScale;
                    else if (node.CurLengthRatio < lengthRatio)
                        node.CurLengthRatio = lengthRatio;
                    usedRatio += node.CurLengthRatio;
                }
                else
                {
                    // Last one
                    node.CurLengthRatio = 1 - usedRatio;
                }
                flexibleIndex++;
            }
            
            float totalLength = (Direction == LayoutDirection.Horizontal ? rect.width : rect.height) - totalDraggableLength - totalFixedLength;
            float usedLength = 0;

            // Draw children GUI
            foreach (var node in Children)
            {
                // Draw child window
                usedLength += DrawChild(node, rect, usedLength, totalLength);
                
                // Draw draggable slider
                usedLength += DrawDraggableSlider(node, rect, usedLength, totalLength, minRatioScale);
            }
        }

        private float DrawChild(WindowNode node, Rect rect, float usedLength, float totalLength)
        {
            // Calculate rect
            Rect childRect = rect;
            float curNodeLength = node.CalcLength(totalLength);
            switch (Direction)
            {
                case LayoutDirection.Horizontal:
                    childRect.x += usedLength;
                    childRect.width = curNodeLength;
                    break;
                case LayoutDirection.Vertical:
                    childRect.y += usedLength;
                    childRect.height = curNodeLength;
                    break;
            }
            // Draw window
            node.Draw(childRect);
            return curNodeLength;
        }

        private float DrawDraggableSlider(WindowNode node, Rect rect, float usedLength, float totalLength, float minRatioScale)
        {
            if (!node.draggable) return 0;
            int dragTargetNodeIndex = node.dragTargetNodeIndex.Value;
            var dragNode = Children[dragTargetNodeIndex];
            // Draw draggable slider
            var dragRect = rect;
            float delta = 0;
            switch (Direction)
            {
                case LayoutDirection.Horizontal:
                    dragRect.x += usedLength;
                    dragRect.width = DRAGGABLE_SPACE;
                    delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
                    break;
                case LayoutDirection.Vertical:
                    dragRect.y += usedLength;
                    dragRect.height = DRAGGABLE_SPACE;
                    delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeVertical).y;
                    break;
            }

            if (delta != 0)
            {
                float newRatio = Mathf.Clamp01((dragNode.CalcLength(totalLength) + delta) / totalLength);
                // Calc max ratio
                float maxRatio = CalcMaxLengthRatio(dragTargetNodeIndex, minRatioScale);
                // Change cur node size
                dragNode.CurLengthRatio = Mathf.Clamp(newRatio, dragNode.MinLengthRatio * minRatioScale, maxRatio);
                float remainRatio = 1 - CalcUsedLengthRatio(0, dragTargetNodeIndex + 1);
                // Change after node size
                ResizeToEnd(dragTargetNodeIndex + 1, remainRatio);
            }
            return DRAGGABLE_SPACE;
        }

        protected abstract void OnGUI(Rect rect);

        private float CalcLength(float totalLength)
        {
            return IsFixed ? (FixedLength ?? 10) : (totalLength * CurLengthRatio);
        }

        private float CalcMaxLengthRatio(int targetIndex, float minRatioScale)
        {
            float maxRatio = 1;
            for (int index = 0, count = Children.Count; index < count; index++)
            {
                var node = Children[index];
                if (node.IsFixed) continue;
                if (index < targetIndex)
                {
                    // Before target node, record cur ratio. Because they can not change size by this drag
                    maxRatio -= node.CurLengthRatio;
                }
                else if (index > targetIndex)
                {
                    // After target node, record min ratio. Because they can change size by this drag
                    maxRatio -= node.MinLengthRatio * minRatioScale;
                }
            }
            return maxRatio;
        }

        private float CalcUsedLengthRatio(int startIndex, int length)
        {
            float usedLengthRatio = 0;
            int end = Mathf.Min(Children.Count, startIndex + length);
            for (int i = startIndex; i < end; i++)
            {
                var node = Children[i];
                if (node.IsFixed) continue;
                usedLengthRatio += node.CurLengthRatio;
            }
            return usedLengthRatio;
        }

        private void ResizeToEnd(int startIndex, float remainRatio)
        {
            float sum = 0;
            for (int i = startIndex; i < Children.Count; i++)
            {
                var node = Children[i];
                if (node.IsFixed) continue;
                sum += Mathf.Max(node.CurLengthRatio, node.MinLengthRatio);
            }
            for (int i = startIndex, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (node.IsFixed) continue;
                node.CurLengthRatio = node.CurLengthRatio / sum * remainRatio;
            }
        }

        private WindowNode FindNextFlexibleNode(int startIndex)
        {
            for (int i = startIndex, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (!node.IsFixed)
                    return node;
            }
            return null;
        }

        public void Add(WindowNode node)
        {
            Parent = this;
            Children.Add(node);
        }

        /// <summary>
        /// Only remove from children
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Removed node</returns>
        public WindowNode RemoveChild(string name)
        {
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (node.Name == name)
                {
                    Children.RemoveAt(i);
                    return node;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Only find from children
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Found node</returns>
        public WindowNode FindChild(string name)
        {
            foreach (var node in Children)
            {
                if (node.Name == name)
                    return node;
            }
            return null;
        }
        
        /// <summary>
        /// Remove from all nodes, except entry node
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Removed node</returns>
        public WindowNode Remove(string name)
        {
            var target = RemoveChild(name);
            if (target != null) return target;
            foreach (var node in Children)
            {
                target = node.Remove(name);
                if (target != null)
                    return target;
            }
            return null;
        }
        
        /// <summary>
        /// Find from all nodes
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Found node</returns>
        public WindowNode Find(string name)
        {
            if (Name == name) return this;
            foreach (var node in Children)
            {
                var target = node.Find(name);
                if (target != null)
                    return target;
            }
            return null;
        }

        public IEnumerator<WindowNode> GetEnumerator()
        {
            yield return this;
            foreach (var child in Children)
            {
                foreach (var node in child)
                    yield return node;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
