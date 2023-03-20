#if UNITY_2019_3_OR_NEWER
using CustomizationInspector.Runtime;
using UnityEditor;

namespace CustomizationInspector.Editor
{
    [CustomPropertyDrawer(typeof(SerializableReferenceDictionaryBase), true)]
    public class SerializableReferenceDictionaryDrawer : SerializableDictionaryDrawerBase
    {
        public SerializableReferenceDictionaryDrawer() : base(true)
        {
        }
    }
}
#endif
