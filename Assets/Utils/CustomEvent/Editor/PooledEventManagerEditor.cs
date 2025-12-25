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

    // Reflection Fields Cache
    private FieldInfo poolField;
    private FieldInfo usedField;
    private FieldInfo capacityField;
    private FieldInfo logEnabledField;
    
    private void OnEnable()
    {
        // 매니저 관련 필드 정보는 OnEnable에서 미리 캐싱 (성능 최적화)
        var mgrType = typeof(PooledEventManager);
        var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        
        poolField = mgrType.GetField("_pool", flags);
        usedField = mgrType.GetField("_used", flags);
        capacityField = mgrType.GetField("_poolCapacity", flags);
        logEnabledField = mgrType.GetField("LogEnabled", flags);   
    }

    public override void OnInspectorGUI()
    {
        DrawScriptField();

        var mgr = (PooledEventManager)target;
        
        serializedObject.Update();

        DrawDebugOptions(mgr);
        DrawManagerStats(mgr);
        DrawActiveEventsList(mgr);

        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying)
            Repaint();
    }

    /// <summary>
    /// 기본 인스펙터처럼 Script 필드를 그려줍니다.
    /// </summary>
    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        }
    }

    /// <summary>
    /// 디버그 옵션 (로그 토글 등)을 그립니다.
    /// </summary>
    private void DrawDebugOptions(PooledEventManager mgr)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);

        // Reflection을 통해 현재 값 가져오기
        var currentLogState = false;
        if (logEnabledField != null)
        {
            currentLogState = (bool)logEnabledField.GetValue(mgr);
        }

        EditorGUI.BeginChangeCheck();
        
        // 토글 UI
        var newLogState = EditorGUILayout.Toggle("Enable Console Logs", currentLogState);

        if (EditorGUI.EndChangeCheck())
        {
            // 값 변경 시 Reflection으로 값 설정 (Undo 지원 포함)
            Undo.RecordObject(mgr, "Toggle Log Enabled");
            
            if (logEnabledField != null)
            {
                logEnabledField.SetValue(mgr, newLogState);
            }
            
            EditorUtility.SetDirty(mgr);
        }

        // 안내 메시지
        if (newLogState)
        {
            EditorGUILayout.HelpBox("Logs will be printed to Console on Create() and Bind().", MessageType.Info);
        }
    }

    /// <summary>
    /// 풀링 매니저의 상태(Total, Used, Free)를 그립니다.
    /// </summary>
    private void DrawManagerStats(PooledEventManager mgr)
    {
        var pool = poolField?.GetValue(mgr) as IList;
        var used = usedField?.GetValue(mgr) as IList;
        var capacity = capacityField != null ? (int)capacityField.GetValue(mgr) : 0;

        var free = pool?.Count ?? 0;
        var usedCount = used?.Count ?? 0;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Manager Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Capacity: {capacity}");
        EditorGUILayout.LabelField($"Used Items: {usedCount}");
        EditorGUILayout.LabelField($"Free Items: {free}");
    }

    /// <summary>
    /// 활성화된 이벤트 리스트와 검색창을 그립니다.
    /// </summary>
    private void DrawActiveEventsList(PooledEventManager mgr)
    {
        var used = usedField?.GetValue(mgr) as IList;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Active Events List", EditorStyles.boldLabel);

        if (used == null || used.Count == 0) return;

        // --- Search UI ---
        searchText = EditorGUILayout.TextField("Search Target Name", searchText);
        var keyword = searchText.ToLower();
        EditorGUILayout.Space(5);

        // --- ColdData Reflection Setup ---
        // 리스트의 첫 번째 요소로 타입을 파악하여 필드 정보를 가져옴
        var firstItem = used[0];
        if (firstItem == null) return;
        var coldType = firstItem.GetType();

        var targetField = coldType.GetField("DebugTargetObject", BindingFlags.NonPublic | BindingFlags.Instance);
        var invokeObjField = coldType.GetField("DebugInvokeObj", BindingFlags.NonPublic | BindingFlags.Instance);
        var invokeCntField = coldType.GetField("DebugInvokeCount", BindingFlags.NonPublic | BindingFlags.Instance);

        // --- Iterate List ---
        for (var i = 0; i < used.Count; i++)
        {
            var cold = used[i];
            if (cold == null) continue;

            // Reflection으로 값 가져오기
            var targetObj = targetField?.GetValue(cold) as Object;
            var invokeObj = invokeObjField?.GetValue(cold) as GameObject;
            var invokeCnt = invokeCntField != null ? (int)invokeCntField.GetValue(cold) : 0;

            // 검색어 필터링
            if (!string.IsNullOrEmpty(keyword))
            {
                if (targetObj == null || !targetObj.name.ToLower().Contains(keyword))
                    continue;
            }

            // 개별 아이템 그리기
            DrawEventItem(i, targetObj, invokeObj, invokeCnt);
        }
    }

    /// <summary>
    /// 개별 이벤트 아이템의 UI를 그립니다.
    /// </summary>
    private void DrawEventItem(int index, Object targetObj, GameObject invokeObj, int invokeCnt)
    {
        // Header Name
        var foldoutName = targetObj != null ? $"[Bind] {targetObj.name}" : "[Wait] (No Target)";

        if (!foldoutStates.ContainsKey(index)) foldoutStates[index] = false;

        // 타겟이 없으면 노란색 강조
        var originalColor = GUI.contentColor;
        if (targetObj == null) GUI.contentColor = Color.yellow;

        foldoutStates[index] = EditorGUILayout.Foldout(foldoutStates[index], foldoutName, true);
        GUI.contentColor = originalColor;

        if (!foldoutStates[index]) return;

        // 상세 내용
        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.ObjectField("Bound Target", targetObj, typeof(Object), true);

            if (invokeCnt > 0)
            {
                EditorGUILayout.LabelField($"Invoked Count: {invokeCnt}");
                if (invokeObj != null)
                {
                    EditorGUILayout.ObjectField("Last Obj", invokeObj, typeof(GameObject), true);
                }
            }
            else
            {
                EditorGUILayout.LabelField("Not invoked yet.");
            }
        }
        EditorGUILayout.EndVertical();
    }
}