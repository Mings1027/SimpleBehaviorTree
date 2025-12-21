#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utils.ObjectPool.Editor
{
    [CustomEditor(typeof(ObjectPoolManager))]
    public class ObjectPoolManagerEditor : UnityEditor.Editor
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
            var type = typeof(ObjectPoolManager);
            var flags = BindingFlags.NonPublic | BindingFlags.Static;

            var poolTableField = type.GetField("poolTable", flags);
            var entryTableField = type.GetField("entryTable", flags); // 구분 기준
            var poolRootsField = type.GetField("poolRoots", flags);

            var pools = poolTableField?.GetValue(null) as IDictionary;
            var entries = entryTableField?.GetValue(null) as IDictionary; // Registry 등록 여부 확인용
            var poolRoots = poolRootsField?.GetValue(null) as Dictionary<GameObject, Transform>;

            if (pools == null || pools.Count == 0)
            {
                EditorGUILayout.HelpBox("No runtime pools.", MessageType.Info);
                return;
            }

            var registryProp = serializedObject.FindProperty("poolRegistry");
            var registry = registryProp.objectReferenceValue as PoolRegistry;

            var registryPrefabs = new HashSet<GameObject>();
            if (registry != null)
            {
                foreach (var entry in registry.Entries)
                {
                    if (entry.Prefab != null) registryPrefabs.Add(entry.Prefab);
                }
            }

            // 2. 두 그룹으로 분류하기
            var registryList = new List<DictionaryEntry>();
            var dynamicList = new List<DictionaryEntry>();

            foreach (DictionaryEntry kv in pools)
            {
                var prefab = kv.Key as GameObject;
                if (prefab == null) continue;

                // entryTable에 키가 있으면 SO에서 온 것, 없으면 코드로 생성된 것
                if (registryPrefabs.Contains(prefab))
                {
                    registryList.Add(kv);
                }
                else
                {
                    dynamicList.Add(kv);
                }
            }

            // 3. 그룹별로 그리기
            DrawPoolSection("Registered From ScriptableObject", registryList, poolRoots, entries);
            DrawPoolSection("Registered From Code", dynamicList, poolRoots, entries);
        }

        private void DrawPoolSection(string title, List<DictionaryEntry> list,
            Dictionary<GameObject, Transform> poolRoots, IDictionary entries)
        {
            if (list.Count == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            // 들여쓰기를 살짝 주어 섹션 구분
            EditorGUI.indentLevel++;

            foreach (var kv in list)
            {
                var prefab = kv.Key as GameObject;
                var inactiveStack = kv.Value as ICollection;

                if (prefab == null || inactiveStack == null)
                    continue;

                int inactiveCount = inactiveStack.Count;
                int activeCount = CountActiveInstances(prefab);

                var useAuto = false;
                var delay = 0f;

                if (entries != null && entries.Contains(prefab))
                {
                    var entryObj = entries[prefab];
                    if (entryObj is PoolEntry entry)
                    {
                        useAuto = entry.UseAutoRelease;
                        delay = entry.AutoReleaseDelay;
                    }
                }

                Transform rootTr = null;
                poolRoots?.TryGetValue(prefab, out rootTr);

                EditorGUILayout.BeginVertical("box");

                // 프리팹 표시
                EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

                // 루트 오브젝트가 있으면 표시
                if (rootTr != null)
                {
                    EditorGUILayout.ObjectField("Pool Root", rootTr.gameObject, typeof(GameObject), true);
                }

                // 상태 표시
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Active: {activeCount}");
                EditorGUILayout.LabelField($"Inactive: {inactiveCount}");
                EditorGUILayout.EndHorizontal();

                if (useAuto)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Auto Release: {delay:0.##} sec");
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
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
}
#endif