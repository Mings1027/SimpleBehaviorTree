using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using BehaviorTree;

public class BehaviorTreeViewer : EditorWindow
{
    private BehaviorTreeController _controller;
    private ScrollView _treeContainer;
    private readonly Dictionary<Node, VisualElement> _nodeElements = new();

    [MenuItem("Tools/Behavior Tree Viewer")]
    public static void Open() => GetWindow<BehaviorTreeViewer>("BT Viewer");

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f); // 다크 테마 배경색

        var controllerField = new ObjectField("Controller") { objectType = typeof(BehaviorTreeController), allowSceneObjects = true };
        controllerField.style.paddingTop = 5;
        controllerField.RegisterValueChangedCallback(evt => { _controller = evt.newValue as BehaviorTreeController; RefreshTreeView(); });
        root.Add(controllerField);

        _treeContainer = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        _treeContainer.style.flexGrow = 1;
        root.Add(_treeContainer);

        root.schedule.Execute(UpdateNodeStates).Every(100);
    }

    private void RefreshTreeView()
    {
        _treeContainer.Clear();
        _nodeElements.Clear();
        if (_controller?.RootNode == null) return;

        // 최상단 여백
        VisualElement spacer = new VisualElement();
        spacer.style.height = 15;
        _treeContainer.Add(spacer);

        DrawNodeRecursive(_controller.RootNode, _treeContainer, 0, false);
    }

    private void DrawNodeRecursive(Node node, VisualElement parentContainer, int depth, bool isLastChild)
    {
        // --- [사이즈 및 간격 조절 변수] ---
        float rowHeight = 40f;      // 줄 높이
        float boxHeight = 30f;      // 노드 박스 높이
        float indentWidth = 24f;    // 들여쓰기 너비
        // ------------------------------

        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.height = rowHeight;
        row.style.alignItems = Align.Center;

        // 1. 수직 가이드 라인 (깊이만큼 반복)
        for (int i = 0; i < depth; i++)
        {
            VisualElement guide = new VisualElement();
            guide.style.width = indentWidth;
            guide.style.height = Length.Percent(100);
            guide.style.justifyContent = Justify.Center;
            guide.style.alignItems = Align.Center;

            // 마지막 깊이에서만 포인트를 줌
            if (i == depth - 1)
            {
                // 세로 가이드 바
                VisualElement vLine = new VisualElement();
                vLine.style.width = 1;
                vLine.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
                vLine.style.position = Position.Absolute;
                vLine.style.top = 0;
                vLine.style.bottom = isLastChild ? rowHeight / 2f : 0;
                guide.Add(vLine);

                // 노드 연결 점 (Dot)
                VisualElement dot = new VisualElement();
                dot.style.width = 4;
                dot.style.height = 4;
                dot.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f);
                dot.style.borderTopLeftRadius = 2;
                dot.style.borderTopRightRadius = 2;
                dot.style.borderBottomLeftRadius = 2;
                dot.style.borderBottomRightRadius = 2;
                dot.style.position = Position.Absolute;
                dot.style.top = rowHeight / 2f - 2f;
                guide.Add(dot);
            }
            row.Add(guide);
        }

        // 2. 노드 박스 (카드 스타일)
        VisualElement nodeBox = new VisualElement();
        nodeBox.style.flexDirection = FlexDirection.Row;
        nodeBox.style.height = boxHeight;
        nodeBox.style.minWidth = 140;
        nodeBox.style.paddingLeft = 8;
        nodeBox.style.paddingRight = 8;
        nodeBox.style.alignItems = Align.Center;
        nodeBox.style.backgroundColor = new Color(0.22f, 0.22f, 0.22f); // 약간 밝은 회색
        nodeBox.style.borderTopLeftRadius = 3;
        nodeBox.style.borderTopRightRadius = 3;
        nodeBox.style.borderBottomLeftRadius = 3;
        nodeBox.style.borderBottomRightRadius = 3;
        
        // 타입별 좌측 포인트 라인
        ApplyNodeTypeStyle(node, nodeBox);

        // 이름 라벨
        Label label = new Label(node.GetType().Name);
        label.style.fontSize = 12;
        label.style.color = new Color(0.85f, 0.85f, 0.85f);
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.flexGrow = 1;
        nodeBox.Add(label);

        // 상태 아이콘 전용 컨테이너
        VisualElement statusIcon = new VisualElement();
        statusIcon.style.width = 16;
        statusIcon.style.height = 16;
        nodeBox.Add(statusIcon);

        // 더블 클릭 이벤트
        nodeBox.RegisterCallback<PointerDownEvent, Node>(OnNodeDoubleClick, node);

        row.Add(nodeBox);
        parentContainer.Add(row);
        _nodeElements[node] = nodeBox;

        // 3. 자식 노드 재귀
        if (node is CompositeNode comp)
        {
            var children = comp.Children;
            for (int i = 0; i < children.Count; i++)
            {
                DrawNodeRecursive(children[i], parentContainer, depth + 1, i == children.Count - 1);
            }
        }
    }

    private void ApplyNodeTypeStyle(Node node, VisualElement box)
    {
        box.style.borderLeftWidth = 3;
        if (node is SequenceNode) box.style.borderLeftColor = new Color(0.2f, 0.5f, 0.8f); // Blue
        else if (node is SelectorNode) box.style.borderLeftColor = new Color(0.9f, 0.4f, 0.2f); // Orange
        else if (node is ParallelNode) box.style.borderLeftColor = new Color(0.3f, 0.7f, 0.3f); // Green
        else box.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
    }

    private void UpdateNodeStates()
    {
        if (_controller == null) return;
        foreach (var kvp in _nodeElements)
        {
            Node node = kvp.Key;
            VisualElement box = kvp.Value;
            var icon = box.ElementAt(1); // statusIcon 컨테이너

            if (_controller.UpdatedThisFrame.Contains(node))
            {
                _controller.LastResults.TryGetValue(node, out var state);
                icon.style.backgroundImage = GetStateIcon(state);
            }
            else
            {
                icon.style.backgroundImage = null;
            }
        }
    }

    private Texture2D GetStateIcon(NodeState state) => state switch {
        NodeState.Running => EditorGUIUtility.IconContent("d_WaitSpin00").image as Texture2D,
        NodeState.Success => EditorGUIUtility.IconContent("TestPassed").image as Texture2D,
        NodeState.Failure => EditorGUIUtility.IconContent("TestFailed").image as Texture2D,
        _ => null
    };

    private void OnNodeDoubleClick(PointerDownEvent evt, Node node)
    {
        if (evt.clickCount == 2)
        {
            OpenNodeScript(node);
        }
    }
    
    private void OpenNodeScript(Node node)
    {
        Type type = node.GetType();
        string[] guids = AssetDatabase.FindAssets($"{type.Name} t:Script");
        foreach (var guid in guids)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
            if (script != null && script.GetClass() == type) { AssetDatabase.OpenAsset(script); return; }
        }
    }
}
