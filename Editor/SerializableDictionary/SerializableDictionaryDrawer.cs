using CustomizationInspector.Runtime;
using UnityEditor;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    public class SerializableDictionaryDrawer : SerializableDictionaryDrawerBase
    {
        public SerializableDictionaryDrawer() : base(true)
        {
        }
    }
}
