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
        public float? FixedLength;
        public bool IsFixed => FixedLength.HasValue;

        private float? curLengthRatio;

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
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (!node.IsFixed)
                {
                    flexibleCount++;
                    totalMinRatio += node.MinLengthRatio;
                }
                else
                    totalFixedLength += node.FixedLength.Value;
                if (i < count - 1)
                {
                    var nextNode = Children[i + 1];
                    if (!node.IsFixed && !nextNode.IsFixed)
                        totalDraggableLength += DRAGGABLE_SPACE;
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
                    if (!node.curLengthRatio.HasValue)
                        node.curLengthRatio = node.MinLengthRatio * initScale;
                    else if (node.curLengthRatio < lengthRatio)
                        node.curLengthRatio = lengthRatio;
                    usedRatio += node.curLengthRatio.Value;
                }
                else
                {
                    // Last one
                    node.curLengthRatio = 1 - usedRatio;
                }
                flexibleIndex++;
            }
            
            float totalLength = (Direction == LayoutDirection.Horizontal ? rect.width : rect.height) - totalDraggableLength - totalFixedLength;
            float usedLength = 0;

            // Draw children GUI
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
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
                usedLength += curNodeLength;
                // Draw
                node.Draw(childRect);
                
                // Add draggable space
                bool draggable = false;
                if (i < count - 1)
                {
                    var nextNode = Children[i + 1];
                    if (!node.IsFixed && !nextNode.IsFixed)
                        draggable = true;
                }

                if (draggable)
                {
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
                        float newRatio = Mathf.Clamp01((curNodeLength + delta) / totalLength);
                        // Calc max ratio
                        float maxRatio = CalcMaxLengthRatio(i, minRatioScale);
                        // Change cur node size
                        node.curLengthRatio = Mathf.Clamp(newRatio, node.MinLengthRatio * minRatioScale, maxRatio);
                        float remainRatio = 1 - CalcUsedLengthRatio(0, i + 1);
                        // Change after node size
                        ResizeToEnd(i + 1, remainRatio);
                    }
                    usedLength += DRAGGABLE_SPACE;
                }
            }
        }

        protected abstract void OnGUI(Rect rect);

        private float CalcLength(float totalLength)
        {
            return IsFixed ? (FixedLength ?? 10) : (totalLength * (curLengthRatio ?? MinLengthRatio));
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
                    maxRatio -= node.curLengthRatio ?? 0;
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
                usedLengthRatio += node.curLengthRatio ?? 0;
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
                sum += Mathf.Max(node.curLengthRatio ?? 0, node.MinLengthRatio);
            }
            for (int i = startIndex, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                if (node.IsFixed) continue;
                node.curLengthRatio = node.curLengthRatio / sum * remainRatio;
            }
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
