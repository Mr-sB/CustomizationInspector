using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public class ContainerNode : WindowNode, IEnumerable<WindowNode>
    {
        public enum LayoutDirection
        {
            Horizontal,
            Vertical,
        }
        
        public const float RESIZE_DRAGGABLE_SPACE = 4;

        /// <summary>
        /// Only root ContainerNode has owner
        /// </summary>
        internal WindowTree owner;
        public override WindowTree Owner {
            get
            {
                if (Parent == null && owner != null)
                    return owner;
                return base.Owner;
            }
        }
        public readonly List<WindowNode> Children;
        public LayoutDirection Direction;
        // public bool AutoRemovable { private set; get; }

        public static WindowNodeOption WithDirection(LayoutDirection direction)
        {
            return node =>
            {
                if (node is ContainerNode containerNode)
                    containerNode.Direction = direction;
            };
        }
        
        // public static WindowNodeOption WithAutoRemovable(bool autoRemovable)
        // {
        //     return node =>
        //     {
        //         if (node is ContainerNode containerNode)
        //             containerNode.AutoRemovable = autoRemovable;
        //     };
        // }
        
        public ContainerNode(string name, params WindowNodeOption[] options) : base(name)
        {
            Children = new List<WindowNode>();
            Direction = LayoutDirection.Horizontal;
            // AutoRemovable = true;
            InitOptions(options);
        }

        protected override void OnGUI(Rect rect)
        {
            // Calc gui rect
            float totalMinRatio = 0;
            float totalFixedLength = 0;
            float totalDraggableLength = 0;
            int flexibleCount = 0;
            int? preFlexibleNodeIndex = null;
            for (int i = 0, count = Children.Count; i < count; i++)
            {
                var node = Children[i];
                node.resizeDragTargetNodeIndex = null;
                if (node.IsFixed)
                {
                    // Between two flexible node
                    if (preFlexibleNodeIndex.HasValue && FindNextFlexibleNode(i + 1) != null)
                    {
                        node.resizeDragTargetNodeIndex = preFlexibleNodeIndex; // Fixed node, drag previous flexible node
                        totalDraggableLength += RESIZE_DRAGGABLE_SPACE;
                    }
                    
                    totalFixedLength += node.FixedLength.Value;
                }
                else
                {
                    var nextFlexibleNode = FindNextFlexibleNode(i + 1);
                    // In front of flexible node
                    if (nextFlexibleNode != null)
                    {
                        node.resizeDragTargetNodeIndex = i; // Flexible node, drag itself
                        totalDraggableLength += RESIZE_DRAGGABLE_SPACE;
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
                
                // Draw resize draggable slider
                usedLength += DrawResizeSlider(node, rect, usedLength, totalLength, minRatioScale);
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
        
        private float DrawResizeSlider(WindowNode node, Rect rect, float usedLength, float totalLength, float minRatioScale)
        {
            if (!node.resizable) return 0;
            int dragTargetNodeIndex = node.resizeDragTargetNodeIndex.Value;
            var dragNode = Children[dragTargetNodeIndex];
            // Draw draggable slider
            var dragRect = rect;
            float delta = 0;
            switch (Direction)
            {
                case LayoutDirection.Horizontal:
                    dragRect.x += usedLength;
                    dragRect.width = RESIZE_DRAGGABLE_SPACE;
                    delta = EditorGUIExtensions.SlideRect(dragRect, MouseCursor.ResizeHorizontal).x;
                    break;
                case LayoutDirection.Vertical:
                    dragRect.y += usedLength;
                    dragRect.height = RESIZE_DRAGGABLE_SPACE;
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
            return RESIZE_DRAGGABLE_SPACE;
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
        
        private void ResizeAll()
        {
            ResizeToEnd(0, 1);
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
            InsertAt(-1, node);
        }
        
        public void InsertAt(int index, WindowNode node)
        {
            node.Parent = this;
            if (index < 0 || index >= Children.Count)
                Children.Add(node);
            else
                Children.Insert(index, node);
            ResizeAll();
        }

        public int IndexOf(WindowNode node)
        {
            return Children.IndexOf(node);
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
                    node.Parent = null;
                    Children.RemoveAt(i);
                    ResizeAll();
                    return node;
                }
            }
            return null;
        }
        
        public void RemoveChild(WindowNode node)
        {
            if (Children.Remove(node))
            {
                node.Parent = null;
                ResizeAll();
            }
        }
        
        // public void RemoveChildOnly(WindowNode node)
        // {
        //     if (Children.Remove(node) && node is ContainerNode containerNode && containerNode.Children.Count > 0 && containerNode.Parent != null)
        //     {
        //         foreach (var child in containerNode.Children)
        //             containerNode.Parent.Add(child);
        //         containerNode.Children.Clear();
        //     }
        // }
        
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
                if (node is ContainerNode containerNode)
                {
                    target = containerNode.Remove(name);
                    if (target != null)
                        return target;
                }
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
                if (node.Name == name)
                    return node;
                if (node is ContainerNode containerNode)
                {
                    var target = containerNode.Find(name);
                    if (target != null)
                        return target;
                }
            }
            return null;
        }

        public override void Query(Rect queryRect, [NotNull] HashSet<WindowNode> results)
        {
            if (!Position.Overlaps(queryRect)) return;
            foreach (var child in Children)
                child.Query(queryRect, results);
        }

        public IEnumerator<WindowNode> GetEnumerator()
        {
            yield return this;
            foreach (var child in Children)
            {
                if (child is ContainerNode containerNode)
                    foreach (var node in containerNode)
                        yield return node;
                else
                    yield return child;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}