#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(ObjectPoolManager))]
public class ObjectPoolManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Pool State", EditorStyles.boldLabel);

        DrawRuntimePools();
    }

    private void DrawRuntimePools()
    {
        var poolsField = typeof(ObjectPoolManager)
            .GetField("pools", BindingFlags.NonPublic | BindingFlags.Static);

        var poolRootsField = typeof(ObjectPoolManager)
            .GetField("poolRoots", BindingFlags.NonPublic | BindingFlags.Static);

        var pools = poolsField?.GetValue(null) as IDictionary;
        var poolRoots = poolRootsField?.GetValue(null) as Dictionary<GameObject, Transform>;

        if (pools == null || pools.Count == 0)
        {
            EditorGUILayout.HelpBox("No runtime pools.", MessageType.Info);
            return;
        }

        foreach (DictionaryEntry kv in pools)
        {
            var prefab = kv.Key as GameObject;
            var pool = kv.Value;

            if (prefab == null || pool == null)
                continue;

            // Inactive count
            var inactiveField = pool.GetType().GetField("Inactive");
            var inactiveStack = inactiveField?.GetValue(pool) as ICollection;
            int inactiveCount = inactiveStack?.Count ?? 0;

            // Active count
            int activeCount = CountActiveInstances(prefab);

            // Pool Root
            Transform rootTr = null;
            if (poolRoots != null)
                poolRoots.TryGetValue(prefab, out rootTr);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

            if (rootTr != null)
                EditorGUILayout.ObjectField("Pool Root", rootTr.gameObject, typeof(GameObject), true);

            EditorGUILayout.LabelField($"Active: {activeCount}    Inactive: {inactiveCount}");

            EditorGUILayout.EndVertical();
        }
    }


    private int CountActiveInstances(GameObject prefab)
    {
        int count = 0;

        var all = FindObjectsByType<PooledObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var pooled in all)
        {
            if (pooled.gameObject.activeInHierarchy &&
                pooled.gameObject.name == prefab.name)
            {
                count++;
            }
        }

        return count;
    }
}
#endif