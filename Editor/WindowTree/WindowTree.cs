using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public class WindowTree : IEnumerable<WindowNode>
    {
        public const string AutoContainerNodeName = "$AutoContainerNode";
        public const float SnapRatio = 0.4f;
        public ContainerNode Root { get; private set; }
        
        private WindowNode draggingNode;
        private int hotControlId;
        private HashSet<WindowNode> queryResults;
        private Rect draggingRect;
        private List<(int index, float distance)> edgeDistanceList = new List<(int index, float distance)>();

        public WindowTree(ContainerNode root)
        {
            Root = root;
            root.owner = this;
        }

        public void Draw(Rect rect)
        {
            GUILayout.BeginArea(rect);
            rect = new Rect(Vector2.zero, rect.size);
            HandleDragging(rect);
            Root.Draw(rect);
            DrawDragging(rect);
            GUILayout.EndArea();
        }

        private void HandleDragging(Rect rect)
        {
            if (draggingNode == null) return;

            if (GUIUtility.hotControl == hotControlId && Event.current.type == UnityEngine.EventType.MouseDrag)
            {
                draggingRect = new Rect
                {
                    size = new Vector2(100, 100),
                    center = Event.current.mousePosition + rect.position
                };
                Event.current.Use();
            }
            // Check if the drag and drop operation has ended
            // Events outside the window range will be set to Ignore
            else if (Event.current.type == UnityEngine.EventType.MouseUp || Event.current.type == UnityEngine.EventType.Ignore)
                StopReplaceDrag(rect);
        }
        
        private void DrawDragging(Rect rect)
        {
            if (draggingNode == null) return;
            
            var (closestNode, index) = CalcSnapNodeEdge(rect);
            if (closestNode == null)
            {
                // Draw the content displayed by dragging
                GUI.Box(draggingRect, draggingNode.Name);
            }
            else
            {
                // Draw snap
                Rect drawRect = closestNode.Position;
                switch (index)
                {
                    case 0: // left
                        drawRect.width *= SnapRatio;
                        break;
                    case 1: // right
                        drawRect.xMin += drawRect.width * (1 - SnapRatio);
                        break;
                    case 2: // top
                        drawRect.height *= SnapRatio;
                        break;
                    case 3: // down
                        drawRect.yMin += drawRect.height * (1 - SnapRatio);
                        break;
                    default:
                        drawRect = Rect.zero;
                        break;
                }
                GUI.Box(drawRect, draggingNode.Name);
            }
        }

        /// <summary>
        /// Call this method to enable replace function 
        /// </summary>
        /// <returns></returns>
        internal void StartReplaceDrag(WindowNode node)
        {
            draggingNode = node;
            
            hotControlId = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.hotControl = hotControlId;
            EditorGUIUtility.SetWantsMouseJumping(1);
            Event.current.Use();
        }
        
        private void StopReplaceDrag(Rect rect)
        {
            GUIUtility.hotControl = 0;
            EditorGUIUtility.SetWantsMouseJumping(0);
            Event.current.Use();
            
            var (closestNode, index) = CalcSnapNodeEdge(rect);
            if (closestNode != null)
            {
                bool insertBefore = false;
                ContainerNode.LayoutDirection direction = ContainerNode.LayoutDirection.Horizontal;
                switch (index)
                {
                    case 0: // left
                        insertBefore = true;
                        direction = ContainerNode.LayoutDirection.Horizontal;
                        break;
                    case 1: // right
                        insertBefore = false;
                        direction = ContainerNode.LayoutDirection.Horizontal;
                        break;
                    case 2: // top
                        insertBefore = true;
                        direction = ContainerNode.LayoutDirection.Vertical;
                        break;
                    case 3: // down
                        insertBefore = false;
                        direction = ContainerNode.LayoutDirection.Vertical;
                        break;
                }
                // Replace window node
                AutoRemove(draggingNode);
                AutoAdd(draggingNode, closestNode, insertBefore, direction);
            }
            
            queryResults.Clear();
            draggingNode = null;
            hotControlId = 0;
            draggingRect = Rect.zero;
        }

        private (WindowNode node, int edgeIndex) CalcSnapNodeEdge(Rect rect)
        {
            // Check the location, whether it can be placed in a suitable position
            Rect queryRect = new Rect
            {
                size = new Vector2(100, 100),
                center = Event.current.mousePosition + rect.position
            };
            // If pointer is in draggingNode range, do not snap
            if (draggingNode.Position.Overlaps(new Rect(queryRect.center, Vector2.zero)))
                return (null, -1);
            
            queryResults = Query(queryRect, queryResults);
            // Cannot be placed on oneself
            queryResults.Remove(draggingNode);
            
            WindowNode closestNode = null;
            float minDistance = 0;
            var center = draggingRect.center;
            float leftRatio = 0;
            float rightRatio = 0;
            float topRatio = 0;
            float downRatio = 0;
            foreach (var node in queryResults)
            {
                if (!node.Replaceable) continue;
                leftRatio = Mathf.Abs(center.x - node.Position.xMin) / node.Position.width;
                rightRatio = Mathf.Abs(center.x - node.Position.xMax) / node.Position.width;
                topRatio = Mathf.Abs(center.y - node.Position.yMin) / node.Position.height;
                downRatio = Mathf.Abs(center.y - node.Position.yMax) / node.Position.height;
                // Not in snap ration range
                if (leftRatio > SnapRatio && rightRatio > SnapRatio && topRatio > SnapRatio && downRatio > SnapRatio) continue;
                
                // Compare the four sides and the center point, and select the one closest to it.
                if (closestNode == null)
                {
                    closestNode = node;
                    minDistance = Mathf.Min(Mathf.Abs(center.x - node.Position.xMin), Mathf.Abs(center.x - node.Position.xMax),
                        Mathf.Abs(center.y - node.Position.yMin), Mathf.Abs(center.y - node.Position.yMax));
                }
                else
                {
                    var distance = Mathf.Min(Mathf.Abs(center.x - node.Position.xMin), Mathf.Abs(center.x - node.Position.xMax),
                        Mathf.Abs(center.y - node.Position.yMin), Mathf.Abs(center.y - node.Position.yMax));
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestNode = node;
                    }
                }
            }

            if (closestNode == null)
                return (null, -1);
            
            // Detect which edge is closest
            float left = Mathf.Abs(center.x - closestNode.Position.xMin);
            float right = Mathf.Abs(center.x - closestNode.Position.xMax);
            float top = Mathf.Abs(center.y - closestNode.Position.yMin);
            float down = Mathf.Abs(center.y - closestNode.Position.yMax);
            leftRatio = left / closestNode.Position.width;
            rightRatio = right / closestNode.Position.width;
            topRatio = top / closestNode.Position.height;
            downRatio = down / closestNode.Position.height;
            edgeDistanceList.Clear();
            if (leftRatio <= SnapRatio)
                edgeDistanceList.Add((0, left));
            if (rightRatio <= SnapRatio)
                edgeDistanceList.Add((1, right));
            if (topRatio <= SnapRatio)
                edgeDistanceList.Add((2, top));
            if (downRatio <= SnapRatio)
                edgeDistanceList.Add((3, down));
            if (edgeDistanceList.Count == 0)
                return (null, -1);

            edgeDistanceList.Sort((a, b) =>
            {
                var result = a.distance.CompareTo(b.distance);
                if (result != 0) return result;
                return a.index.CompareTo(b.index);
            });
            return (closestNode, edgeDistanceList[0].index);
        }

        public bool Add(WindowNode node, string parentNodeName = null)
        {
            var parent = string.IsNullOrEmpty(parentNodeName) ? Root : Find(parentNodeName);
            if (parent is not ContainerNode containerNode) return false;
            containerNode.Add(node);
            return true;
        }

        public WindowNode Remove(string name)
        {
            return Root.Remove(name);
        }

        public WindowNode Find(string name)
        {
            return Root.Find(name);
        }
        
        public void AutoAdd(WindowNode node, string toAddNodeName, bool insertBefore, ContainerNode.LayoutDirection direction)
        {
            var toAdd = string.IsNullOrEmpty(toAddNodeName) ? Root : Find(toAddNodeName);
            AutoAdd(node, toAdd, insertBefore, direction);
        }
        
        /// <summary>
        /// Add a node to another node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="toAdd"></param>
        /// <param name="insertBefore"></param>
        /// <param name="direction"></param>
        public void AutoAdd(WindowNode node, WindowNode toAdd, bool insertBefore, ContainerNode.LayoutDirection direction)
        {
            if (toAdd is ContainerNode containerNode)
            {
                if (containerNode.Direction == direction || containerNode.Children.Count == 1)
                {
                    // Change to new direction
                    containerNode.Direction = direction;
                    toAdd.CurLengthRatio = 0.2f;
                    // Deal with the position before and after
                    if (insertBefore)
                        containerNode.InsertAt(0, node);
                    else
                        containerNode.Add(node);
                }
                else
                    SplitNewContainer(containerNode, node, insertBefore, direction);
            }
            else
            {
                if (toAdd.Parent.Direction == direction || toAdd.Parent.Children.Count == 1)
                {
                    // Change to new direction
                    toAdd.Parent.Direction = direction;
                    var totalRatio = toAdd.CurLengthRatio;
                    node.CurLengthRatio = totalRatio * SnapRatio;
                    toAdd.CurLengthRatio = totalRatio * (1 - SnapRatio);
                    // Deal with the position before and after
                    int index = toAdd.Parent.IndexOf(toAdd);
                    if (!insertBefore)
                        index += 1;
                    toAdd.Parent.InsertAt(index, node);
                }
                else
                    SplitNewContainer(toAdd, node, insertBefore, direction);
            }
        }
        
        private static void SplitNewContainer(WindowNode toSplit, WindowNode node, bool insertBefore, ContainerNode.LayoutDirection direction)
        {
            // Create new container
            var parent = toSplit.Parent;
            int index = parent.IndexOf(toSplit);
            parent.RemoveChild(toSplit);
            ContainerNode newContainerNode = new ContainerNode(AutoContainerNodeName,
                //ContainerNode.WithAutoRemovable(true),
                ContainerNode.WithDirection(direction),
                WindowNode.WithCurLengthRatio(toSplit.CurLengthRatio));
            parent.InsertAt(index, newContainerNode);
            node.CurLengthRatio = SnapRatio;
            toSplit.CurLengthRatio = 1 - SnapRatio;
            if (insertBefore)
            {
                newContainerNode.Add(node);
                newContainerNode.Add(toSplit);
            }
            else
            {
                newContainerNode.Add(toSplit);
                newContainerNode.Add(node);
            }
        }

        public WindowNode AutoRemove(string name)
        {
            var node = Find(name);
            AutoRemove(node);
            return node;
        }
        
        /// <summary>
        /// Remove a window node and unused container nodes
        /// </summary>
        /// <param name="node"></param>
        public void AutoRemove(WindowNode node)
        {
            if (node == null) return;
            var iterator = node.Parent;
            iterator.RemoveChild(node);

            // Remove Unused node
            while (/*iterator.AutoRemovable && */iterator.Children.Count <= 0 && iterator?.Parent != null)
            {
                var parent = iterator.Parent;
                parent.RemoveChild(iterator);
                iterator = parent;
            }
        }

        public HashSet<WindowNode> Query(Rect queryRect, HashSet<WindowNode> results)
        {
            if (results == null)
                results = new HashSet<WindowNode>();
            else
                results.Clear();
            Root.Query(queryRect, results);
            return results;
        }

        public IEnumerator<WindowNode> GetEnumerator()
        {
            return Root.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
