using UnityEditor;
using UnityEngine;

namespace Utils.ObjectPool.Editor
{
    [CustomPropertyDrawer(typeof(PoolEntry))]
    public class PoolEntryDrawer : PropertyDrawer
    {
        private float LineHeight => EditorGUIUtility.singleLineHeight + 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                totalHeight += LineHeight * 3;
                var useAuto = property.FindPropertyRelative("UseAutoRelease");
                if (useAuto.boolValue)
                {
                    totalHeight += LineHeight;
                }
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                rect.y += LineHeight;

                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Prefab"));
                rect.y += LineHeight;

                EditorGUI.PropertyField(rect, property.FindPropertyRelative("PreloadCount"));
                rect.y += LineHeight;

                var useAuto = property.FindPropertyRelative("UseAutoRelease");
                EditorGUI.PropertyField(rect, useAuto);
                rect.y += LineHeight;

                if (useAuto.boolValue)
                {
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("AutoReleaseDelay"));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }
    }
}