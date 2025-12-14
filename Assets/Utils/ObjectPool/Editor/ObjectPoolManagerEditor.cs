using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPoolManager))]
public class ObjectPoolManagerEditor : Editor
{
    private bool _showRuntimePools = true;
    private GameObject _searchPrefab;
    private readonly Dictionary<GameObject, bool> _poolFoldouts = new();

    private void OnEnable()
    {
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

        DrawPrefabLookup();
        DrawRuntimePools();
    }

    // ===============================
    // Prefab Lookup
    // ===============================
    private void DrawPrefabLookup()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Prefab Lookup", EditorStyles.boldLabel);

        _searchPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Search Prefab",
            _searchPrefab,
            typeof(GameObject),
            false
        );

        if (_searchPrefab == null)
            return;

        var manager = (ObjectPoolManager)target;

        bool inPreset = manager.PresetsContains(_searchPrefab);
        bool inRuntime = ObjectPoolManager.IsRegistered(_searchPrefab);

        if (inPreset)
        {
            EditorGUILayout.HelpBox("Preset registered", MessageType.Info);
            return;
        }

        if (inRuntime)
        {
            EditorGUILayout.HelpBox("Runtime registered (code)", MessageType.Warning);
            return;
        }

        EditorGUILayout.HelpBox("Not registered", MessageType.Error);
    }


    // ===============================
    // Runtime Pool Debug
    // ===============================
    private void DrawRuntimePools()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Runtime Pools (Debug)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Runtime pool info is available in Play Mode only.",
                MessageType.Info
            );
            return;
        }

        _showRuntimePools = EditorGUILayout.Foldout(
            _showRuntimePools,
            "Active Pools",
            true
        );

        if (!_showRuntimePools)
            return;

        var info = ObjectPoolManager.GetPoolDebugInfo();

        if (info.Count == 0)
        {
            EditorGUILayout.LabelField("No pools registered.");
            return;
        }

        foreach (var kv in info)
        {
            var prefab = kv.Key;
            var (total, inactive) = kv.Value;
            int active = total - inactive;

            if (!_poolFoldouts.ContainsKey(prefab))
                _poolFoldouts[prefab] = false;

            bool highlight =
                _searchPrefab != null &&
                prefab == _searchPrefab;

            using (new EditorGUILayout.VerticalScope(
                       highlight ? "SelectionRect" : "box"))
            {
                DrawPoolFoldout(prefab, total, active, inactive);
            }
        }
    }

    private void DrawPoolFoldout(GameObject prefab, int total, int active, int inactive)
    {
        bool isExpanded = _poolFoldouts[prefab];

        string header = isExpanded ? prefab.name : $"{prefab.name}    T:{total}  A:{active}  I:{inactive}";

        _poolFoldouts[prefab] = EditorGUILayout.BeginFoldoutHeaderGroup(
            isExpanded,
            header
        );

        if (_poolFoldouts[prefab])
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.ObjectField(
                "Prefab",
                prefab,
                typeof(GameObject),
                false
            );

            EditorGUILayout.LabelField("Total", total.ToString());
            EditorGUILayout.LabelField("Active", active.ToString());
            EditorGUILayout.LabelField("Inactive", inactive.ToString());

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}