using UnityEditor;
using UnityEngine;
using PhotoMode;

namespace PhotoMode
{
#if UNITY_EDITOR
    // IngredientDrawer
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            float quaterWidth = position.width * 0.25f;
            float halfWidth = position.width * 0.5f;

            Rect minRect = new Rect(position.x, position.y, 30, position.height);
            Rect minValueRect = new Rect(position.x + 30, position.y, halfWidth - 35, position.height);
            Rect maxRect = new Rect(position.xMax - halfWidth + 5, position.y, 30, position.height);
            Rect maxValueRect = new Rect(position.x + halfWidth + 35, position.y, halfWidth - 35, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(minRect, "Min");
            EditorGUI.PropertyField(minValueRect, property.FindPropertyRelative("min"), GUIContent.none);
            EditorGUI.LabelField(maxRect, "Max");
            EditorGUI.PropertyField(maxValueRect, property.FindPropertyRelative("max"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
#endif
}