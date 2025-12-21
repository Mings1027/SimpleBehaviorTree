using UnityEditor;
using UnityEngine;

namespace Utils.ObjectPool.Editor
{
    [CustomEditor(typeof(PoolRegistry))]
    public class PoolRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty entriesProp;

        // 검색 필드 변수
        private string searchString = "";
        private GameObject searchPrefab = null;
        private bool showSearchOptions = true;

        private void OnEnable()
        {
            // PoolRegistry의 Entries 리스트 프로퍼티 가져오기
            entriesProp = serializedObject.FindProperty("entries");
        }

        public override void OnInspectorGUI()
        {
            // 변경 사항 감지 시작
            serializedObject.Update();

            // 1. 타이틀 및 검색 옵션 그리기
            DrawSearchHeader();

            // 2. 검색 중인지 확인
            bool isSearching = !string.IsNullOrEmpty(searchString) || searchPrefab != null;

            EditorGUILayout.Space();

            if (isSearching)
            {
                // 검색 중이면 필터링된 결과만 표시
                DrawFilteredList();
            }
            else
            {
                // 검색 중이 아니면 기본 리스트 표시 (순서 변경, 추가/삭제 기능 유지)
                EditorGUILayout.PropertyField(entriesProp, true);
            }

            // 변경 사항 적용
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSearchHeader()
        {
            showSearchOptions = EditorGUILayout.Foldout(showSearchOptions, "Search & Filters", true);

            if (showSearchOptions)
            {
                EditorGUILayout.BeginVertical("box");

                // --- 이름으로 검색 ---
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("By Name", GUILayout.Width(70));
                string newSearchString = EditorGUILayout.TextField(searchString);
                if (GUILayout.Button("Clear", GUILayout.Width(50))) newSearchString = "";
                EditorGUILayout.EndHorizontal();

                // --- 프리팹으로 검색 ---
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("By Prefab", GUILayout.Width(70));
                GameObject newSearchPrefab =
                    (GameObject)EditorGUILayout.ObjectField(searchPrefab, typeof(GameObject), false);
                if (GUILayout.Button("Clear", GUILayout.Width(50))) newSearchPrefab = null;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                // 값이 바뀌었을 때만 업데이트 (불필요한 갱신 방지)
                if (newSearchString != searchString || newSearchPrefab != searchPrefab)
                {
                    searchString = newSearchString;
                    searchPrefab = newSearchPrefab;
                }
            }
        }

        private void DrawFilteredList()
        {
            EditorGUILayout.LabelField("Search Results", EditorStyles.boldLabel);

            int matchCount = 0;
            int listSize = entriesProp.arraySize;

            for (int i = 0; i < listSize; i++)
            {
                SerializedProperty entry = entriesProp.GetArrayElementAtIndex(i);
                SerializedProperty prefabProp = entry.FindPropertyRelative("Prefab");

                GameObject prefab = prefabProp.objectReferenceValue as GameObject;

                if (prefab == null) continue;

                bool match = true;

                // 1. 이름 검색 (대소문자 무시)
                if (!string.IsNullOrEmpty(searchString))
                {
                    if (!prefab.name.ToLower().Contains(searchString.ToLower()))
                    {
                        match = false;
                    }
                }

                // 2. 프리팹 객체 검색
                if (searchPrefab != null)
                {
                    if (prefab != searchPrefab)
                    {
                        match = false;
                    }
                }

                // 조건에 맞으면 그리기
                if (match)
                {
                    matchCount++;

                    EditorGUILayout.BeginVertical("box");

                    // 원래 리스트에서의 인덱스를 표시해두면 나중에 찾기 편함
                    // EditorGUILayout.LabelField($"Index: {i}", EditorStyles.miniLabel);

                    // 해당 항목 그리기 (자식 프로퍼티 포함)
                    EditorGUILayout.PropertyField(entry, true);

                    EditorGUILayout.EndVertical();
                }
            }

            if (matchCount == 0)
            {
                EditorGUILayout.HelpBox("No entries found matching your criteria.", MessageType.Info);
            }
        }
    }
}