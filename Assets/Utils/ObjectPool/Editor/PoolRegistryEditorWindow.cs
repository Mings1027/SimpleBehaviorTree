using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PoolRegistryEditorWindow : EditorWindow
{
    // 에디터 윈도우 메뉴 추가
    [MenuItem("Pooling/Pool Registry Editor")]
    public static void ShowWindow()
    {
        GetWindow<PoolRegistryEditorWindow>("Pool Registry Editor");
    }

    private TwoPaneSplitView _splitView;
    private ListView _leftListView;
    private VisualElement _rightPanel;
    private ObjectField _registryField;
    
    // 선택된 데이터를 수정하기 위한 SerializedObject
    private SerializedObject _serializedRegistry;
    private SerializedProperty _entriesProp;

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // 1. 상단: PoolRegistry 선택 필드
        _registryField = new ObjectField("Target Registry")
        {
            objectType = typeof(PoolRegistry)
        };
        _registryField.RegisterValueChangedCallback(evt =>
        {
            SetTarget(evt.newValue as PoolRegistry);
        });
        root.Add(_registryField);

        // 2. 메인 구조: TwoPaneSplitView (좌/우 분할)
        // SplitView는 UXML 없이 C#에서 바로 생성 가능합니다.
        _splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        _splitView.style.flexGrow = 1; // 화면 꽉 채우기
        root.Add(_splitView);

        // 3. 좌측 패널 (리스트)
        _leftListView = new ListView();
        _leftListView.headerTitle = "Pool Entries";
        _leftListView.showBorder = true;
        _leftListView.makeItem = () => new Label(); // 리스트의 각 항목(VisualElement) 생성
        _leftListView.bindItem = (element, index) =>
        {
            // 리스트에 표시할 텍스트 설정 (Prefab 이름 등)
            if (_entriesProp != null && index < _entriesProp.arraySize)
            {
                var entryProp = _entriesProp.GetArrayElementAtIndex(index);
                var prefabProp = entryProp.FindPropertyRelative("Prefab");
                
                var label = element as Label;
                if (prefabProp.objectReferenceValue != null)
                    label.text = prefabProp.objectReferenceValue.name;
                else
                    label.text = $"Element {index} (Empty)";
            }
        };

        // 리스트 선택 이벤트 등록
        _leftListView.selectionChanged += OnListSelectionChange;
        
        // TwoPaneSplitView에 좌측 패널 추가
        _splitView.Add(_leftListView);

        // 4. 우측 패널 (상세 정보)
        _rightPanel = new ScrollView(ScrollViewMode.Vertical);
        _rightPanel.name = "DetailPanel"; // root.Q<ScrollView>("DetailPanel")로 찾을 수 있게 이름 지정
        _rightPanel.style.paddingLeft = 10;
        _rightPanel.style.paddingRight = 10;
        _rightPanel.style.paddingTop = 10;
        
        // TwoPaneSplitView에 우측 패널 추가
        _splitView.Add(_rightPanel);
        
        // *만약 선택된 오브젝트가 있다면 초기화*
        if (Selection.activeObject is PoolRegistry registry)
        {
            _registryField.value = registry;
            SetTarget(registry);
        }
    }

    // 타겟 PoolRegistry가 변경되었을 때 호출
    private void SetTarget(PoolRegistry registry)
    {
        _rightPanel.Clear();
        _leftListView.itemsSource = null;
        _leftListView.Rebuild();

        if (registry == null) return;

        // SerializedObject 생성 (데이터 수정 및 Undo/Redo 지원을 위해 필수)
        _serializedRegistry = new SerializedObject(registry);
        _entriesProp = _serializedRegistry.FindProperty("entries");

        // ListView 연결
        _leftListView.itemsSource = registry.Entries; // 갯수 파악용 리스트
        _leftListView.Rebuild();
    }

    // 리스트에서 항목을 클릭했을 때 (우측 패널 그리기)
    private void OnListSelectionChange(System.Collections.Generic.IEnumerable<object> selectedItems)
    {
        _rightPanel.Clear(); // 기존 내용 지우기
        
        // 선택된 인덱스 가져오기
        var selectedIndex = _leftListView.selectedIndex;
        if (selectedIndex < 0 || _serializedRegistry == null) return;

        // 해당 인덱스의 SerializedProperty 가져오기
        var selectedEntryProp = _entriesProp.GetArrayElementAtIndex(selectedIndex);

        // UI 타이틀 추가
        var title = new Label("Entry Details");
        title.style.fontSize = 18;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 10;
        _rightPanel.Add(title);

        // PoolEntry 구조체의 각 필드를 그려줌 (PropertyField 사용 시 자동 바인딩)
        CreateAndBindProp(selectedEntryProp, "Prefab");
        CreateAndBindProp(selectedEntryProp, "PreloadCount");
        CreateAndBindProp(selectedEntryProp, "UseAutoRelease");
        CreateAndBindProp(selectedEntryProp, "AutoReleaseDelay");

        // 변경 사항이 있으면 적용 (실시간 반영)
        _rightPanel.TrackPropertyValue(selectedEntryProp, prop => 
        {
            _serializedRegistry.ApplyModifiedProperties();
            _leftListView.RefreshItem(selectedIndex); // 프리팹 변경 시 리스트 이름 갱신용
        });
    }

    // PropertyField를 생성하고 패널에 추가하는 헬퍼 함수
    private void CreateAndBindProp(SerializedProperty parentProp, string relativePath)
    {
        var prop = parentProp.FindPropertyRelative(relativePath);
        if (prop != null)
        {
            var field = new PropertyField(prop);
            field.Bind(_serializedRegistry); // SerializedObject에 바인딩
            _rightPanel.Add(field);
        }
    }
}