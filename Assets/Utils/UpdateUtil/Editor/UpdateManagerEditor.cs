using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(UpdateManager))]
public class UpdateManagerEditor : Editor
{
    private const string ShowUpdateKey = "UpdateManagerEditor.ShowUpdate";
    private const string ShowFixedKey  = "UpdateManagerEditor.ShowFixed";
    private const string ShowLateKey   = "UpdateManagerEditor.ShowLate";

    private FieldInfo _updateField;
    private FieldInfo _fixedField;
    private FieldInfo _lateField;

    private bool _showUpdate = true;
    private bool _showFixed;
    private bool _showLate;

    private void OnEnable()
    {
        var type = typeof(UpdateManager);

        _updateField = type.GetField("_updates", BindingFlags.NonPublic | BindingFlags.Static);
        _fixedField  = type.GetField("_fixedUpdates", BindingFlags.NonPublic | BindingFlags.Static);
        _lateField   = type.GetField("_lateUpdates", BindingFlags.NonPublic | BindingFlags.Static);

        _showUpdate = SessionState.GetBool(ShowUpdateKey, true);
        _showFixed  = SessionState.GetBool(ShowFixedKey, false);
        _showLate   = SessionState.GetBool(ShowLateKey, false);

        EditorApplication.update += OnEditorUpdate;
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (Application.isPlaying)
            Repaint();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Registered Observers", EditorStyles.boldLabel);

        _showUpdate = DrawObserverFoldout("Update", _showUpdate, _updateField);
        SessionState.SetBool(ShowUpdateKey, _showUpdate);

        _showFixed = DrawObserverFoldout("FixedUpdate", _showFixed, _fixedField);
        SessionState.SetBool(ShowFixedKey, _showFixed);

        _showLate = DrawObserverFoldout("LateUpdate", _showLate, _lateField);
        SessionState.SetBool(ShowLateKey, _showLate);

    }

    private bool DrawObserverFoldout(string title, bool foldout, FieldInfo field)
    {
        if (field == null)
            return foldout;

        var list = field.GetValue(null) as IList;
        int count = list != null ? list.Count : 0;

        foldout = EditorGUILayout.Foldout(foldout, $"{title} ({count})", true);
        if (!foldout)
            return foldout;

        EditorGUI.indentLevel++;

        if (list == null || count == 0)
        {
            EditorGUILayout.LabelField("None");
            EditorGUI.indentLevel--;
            return foldout;
        }

        EditorGUI.BeginDisabledGroup(true);

        for (int i = 0; i < count; i++)
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

        return foldout;
    }

}