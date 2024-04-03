using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomizationInspector.Editor
{
    public class WindowTree<T> : IEnumerable<T> where T : WindowNode
    {
        public T Root;

        public WindowTree(T root)
        {
            Root = root;
        }

        public void Draw(Rect rect)
        {
            Root.Draw(rect);
        }

        public bool Add(T node, string parentNodeName = null)
        {
            var parent = string.IsNullOrEmpty(parentNodeName) ? Root : Find(parentNodeName);
            if (parent == null) return false;
            parent.Add(node);
            return true;
        }

        public T Remove(string name)
        {
            return Root.Remove(name) as T;
        }

        public T Find(string name)
        {
            return Root.Find(name) as T;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var node in Root)
                yield return node as T;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
