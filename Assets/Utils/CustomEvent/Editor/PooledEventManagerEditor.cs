using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using CustomEvent;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PooledEventManager))]
public class PooledEventManagerEditor : Editor
{
    private static Dictionary<int, bool> foldoutStates = new();
    private string searchText = "";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var mgr = (PooledEventManager)target;
        var type = typeof(PooledEventManager);

        var poolField = type.GetField("_pool", BindingFlags.NonPublic | BindingFlags.Instance);
        var usedField = type.GetField("_used", BindingFlags.NonPublic | BindingFlags.Instance);
        var capacityField = type.GetField("_poolCapacity", BindingFlags.NonPublic | BindingFlags.Instance);

        var pool = poolField?.GetValue(mgr) as IList;
        var used = usedField?.GetValue(mgr) as IList;
        int capacity = capacityField != null ? (int)capacityField.GetValue(mgr) : 0;

        int free = pool?.Count ?? 0;
        int usedCount = used?.Count ?? 0;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("PooledEvent Manager Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total : {capacity}");
        EditorGUILayout.LabelField($"Used  : {usedCount}");
        EditorGUILayout.LabelField($"Free  : {free}");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Active Events", EditorStyles.boldLabel);

        if (used == null) return;

        EditorGUI.BeginChangeCheck();
        searchText = EditorGUILayout.TextField("Search", searchText);
        EditorGUILayout.Space(5);

        // 검색어를 소문자로 통일
        string keyword = searchText.ToLower();

        for (int i = 0; i < used.Count; i++)
        {
            var cold = used[i];
            if (cold == null) continue;

            var coldType = cold.GetType();

            // Reflection fields
            var targetField = coldType.GetField("DebugTargetObject", BindingFlags.NonPublic | BindingFlags.Instance);
            var invokeObjField = coldType.GetField("DebugInvokeObj", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileField = coldType.GetField("DebugBindFile", BindingFlags.NonPublic | BindingFlags.Instance);
            var lineField = coldType.GetField("DebugBindLine", BindingFlags.NonPublic | BindingFlags.Instance);
            var invokeCntField = coldType.GetField("DebugInvokeCount", BindingFlags.NonPublic | BindingFlags.Instance);

            var targetObj = targetField?.GetValue(cold) as Object;
            var invokeObj = invokeObjField?.GetValue(cold) as GameObject;
            var file = fileField?.GetValue(cold) as string;
            int line = lineField != null ? (int)lineField.GetValue(cold) : 0;
            int invokeCnt = invokeCntField != null ? (int)invokeCntField.GetValue(cold) : 0;

            // Foldout Title 설정
            string foldoutName = targetObj != null ? $"{targetObj.name} Event" : "(No Target) Event";

            // ★ 검색 필터링
            if (!string.IsNullOrEmpty(keyword))
            {
                var match = targetObj != null && targetObj.name.ToLower().Contains(keyword);

                if (!match) continue;
            }

            if (!foldoutStates.ContainsKey(i))
                foldoutStates[i] = false;

            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], foldoutName, true);

            if (!foldoutStates[i])
                continue;

            EditorGUILayout.BeginVertical("box");

            // Target 표시
            EditorGUILayout.ObjectField("Target", targetObj, typeof(Object), true);

            // InvokeEvent(GameObject go) 전달된 객체 표시
            if (invokeObj != null)
            {
                EditorGUILayout.ObjectField("Invoke GameObject", invokeObj, typeof(GameObject), true);
            }

            // Invoke Count 표시
            EditorGUILayout.LabelField("Invoke Count", invokeCnt.ToString());

            // Script Open 버튼만 남김
            if (!string.IsNullOrEmpty(file))
            {
                if (GUILayout.Button("Open Script"))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(file, line > 0 ? line : 1);
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (Application.isPlaying)
            Repaint();
    }
}