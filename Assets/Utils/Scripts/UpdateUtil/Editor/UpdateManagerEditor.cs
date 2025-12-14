using UnityEditor;
using UnityEngine;
using Utils;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(UpdateManager))]
public class UpdateManagerEditor : Editor
{
    private FieldInfo _updateField;
    private FieldInfo _fixedField;
    private FieldInfo _lateField;

    private bool _showUpdate = true;
    private bool _showFixed = false;
    private bool _showLate = false;

    private void OnEnable()
    {
        var type = typeof(UpdateManager);

        _updateField = type.GetField(
            "_updates",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        _fixedField = type.GetField(
            "_fixedUpdates",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        _lateField = type.GetField(
            "_lateUpdates",
            BindingFlags.NonPublic | BindingFlags.Static
        );
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Registered Observers", EditorStyles.boldLabel);

        DrawObserverFoldout("Update", ref _showUpdate, _updateField);
        DrawObserverFoldout("FixedUpdate", ref _showFixed, _fixedField);
        DrawObserverFoldout("LateUpdate", ref _showLate, _lateField);

        Repaint();
    }

    private void DrawObserverFoldout(
        string title,
        ref bool foldout,
        FieldInfo field
    )
    {
        if (field == null)
            return;

        var list = field.GetValue(null) as IList;

        int count = list != null ? list.Count : 0;

        foldout = EditorGUILayout.Foldout(
            foldout,
            $"{title} ({count})",
            true
        );

        if (!foldout)
            return;

        EditorGUI.indentLevel++;

        if (list == null || count == 0)
        {
            EditorGUILayout.LabelField("None");
            EditorGUI.indentLevel--;
            return;
        }

        EditorGUI.BeginDisabledGroup(true); // 🔒 읽기 전용

        for (int i = 0; i < list.Count; i++)
        {
            var obj = list[i];

            if (obj == null)
            {
                EditorGUILayout.LabelField($"[{i}] <null>");
                continue;
            }

            if (obj is MonoBehaviour mb)
            {
                EditorGUILayout.ObjectField(
                    $"[{i}]",
                    mb,
                    typeof(MonoBehaviour),
                    true
                );
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"[{i}]",
                    obj.GetType().Name
                );
            }
        }

        EditorGUI.EndDisabledGroup();
        EditorGUI.indentLevel--;
    }
}
