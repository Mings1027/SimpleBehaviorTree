using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using BehaviorTree;

public class BehaviorTreeViewer : EditorWindow
{
    private BehaviorTreeController controller;

    private Texture2D iconRunning;
    private Texture2D iconSuccess;
    private Texture2D iconFailure;

    private float nodeHeight = 28f;
    private float nodeWidth = 200f;
    private float verticalSpacing = 6f;
    private float indentWidth = 28f;

    private float currentY;

    private Dictionary<Node, Rect> nodeRects = new();

    [MenuItem("Window/BehaviorTree/Tree Viewer")]
    public static void Open()
    {
        GetWindow<BehaviorTreeViewer>("BT Tree Viewer");
    }

    private void OnEnable()
    {
        iconRunning = EditorGUIUtility.IconContent("d_WaitSpin00").image as Texture2D;
        iconSuccess = EditorGUIUtility.IconContent("TestPassed").image as Texture2D;
        iconFailure = EditorGUIUtility.IconContent("TestFailed").image as Texture2D;

        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    private void OnGUI()
    {
        controller = (BehaviorTreeController)EditorGUILayout.ObjectField(
            "Controller", controller, typeof(BehaviorTreeController), true);

        if (controller == null)
        {
            EditorGUILayout.HelpBox("BehaviorTreeController를 선택하세요.", MessageType.Info);
            return;
        }

        if (controller.RootNode == null)
        {
            EditorGUILayout.HelpBox("RootNode가 없습니다.", MessageType.Warning);
            return;
        }

        GUILayout.Space(10);

        currentY = 20f;
        nodeRects.Clear();

        DrawNodeRecursive(controller.RootNode, 0);
    }

    // =====================================================================
    // 노드 재귀 출력
    // =====================================================================
    private void DrawNodeRecursive(Node node, int depth)
    {
        float x = 20 + depth * indentWidth;
        Rect rect = new Rect(x, currentY, nodeWidth, nodeHeight);

        nodeRects[node] = rect;

        DrawNodeBox(rect, node);
        DrawNodeIcons(rect, node);

        currentY += nodeHeight + verticalSpacing;

        var children = node.GetChildren();
        if (children == null) return;

        foreach (var child in children)
        {
            DrawNodeRecursive(child, depth + 1);
            DrawConnection(node, child, depth);
        }
    }

    // =====================================================================
    // 부모 → 자식 직각 연결선
    // =====================================================================
    private void DrawConnection(Node parent, Node child, int depth)
    {
        Rect p = nodeRects[parent];
        Rect c = nodeRects[child];

        float baseX = 20 + depth * indentWidth;      // 부모 들여쓰기 기준
        float verticalX = baseX - 10;                // 세로선 위치

        float parentY = p.center.y;
        float childY = c.center.y;

        Handles.color = Color.gray;

        // 부모 → 세로선까지 가로선
        Handles.DrawLine(
            new Vector2(baseX, parentY),
            new Vector2(verticalX, parentY)
        );

        // 세로선 (부모→자식)
        Handles.DrawLine(
            new Vector2(verticalX, parentY),
            new Vector2(verticalX, childY)
        );

        // 세로선 → 자식 박스까지 가로선
        Handles.DrawLine(
            new Vector2(verticalX, childY),
            new Vector2(baseX + indentWidth, childY)
        );
    }

    // 박스 그리기
    private void DrawNodeBox(Rect rect, Node node)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };
        GUI.Box(rect, node.GetType().Name, style);
    }

    // 아이콘 여러 개 표시
    private void DrawNodeIcons(Rect rect, Node node)
    {
        var icons = GetNodeIcons(node);
        if (icons.Count == 0) return;

        float size = 16f;
        float spacing = 2f;

        for (int i = 0; i < icons.Count; i++)
        {
            int index = icons.Count - 1 - i;

            Rect r = new Rect(
                rect.xMax - size - 4 - (i * (size + spacing)),
                rect.y + (rect.height - size) * 0.5f,
                size, size
            );
            GUI.DrawTexture(r, icons[index]);
        }
    }

    // 이벤트 기반 아이콘 구성
    private List<Texture2D> GetNodeIcons(Node node)
    {
        List<Texture2D> icons = new();

        if (node.UpdatedThisFrame)
            icons.Add(iconRunning);

        if (node.ExitedThisFrame)
        {
            if (node.LastResult == NodeState.Success)
                icons.Add(iconSuccess);
            else
                icons.Add(iconFailure);
        }

        return icons;
    }
}
