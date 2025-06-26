using UnityEditor;
using UnityEngine;

namespace CustomInspectors
{
    [CustomPropertyDrawer(typeof(LayerFieldAttribute))]
    public class LayerFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
    }
}