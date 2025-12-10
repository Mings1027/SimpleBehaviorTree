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

    // 노드 박스 위치
    private readonly Dictionary<Node, Rect> nodeRects = new();

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

    // ------------------------------------------------------------
    // 트리 재귀 출력
    // ------------------------------------------------------------
    private void DrawNodeRecursive(Node node, int depth)
    {
        float x = 20 + depth * indentWidth;
        Rect rect = new Rect(x, currentY, nodeWidth, nodeHeight);

        nodeRects[node] = rect;

        DrawNodeBox(rect, node);
        DrawNodeIcons(rect, node);

        currentY += nodeHeight + verticalSpacing;

        if (node is CompositeNode comp)
        {
            var children = comp.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                DrawNodeRecursive(child, depth + 1);
                DrawConnection(node, child, depth);
            }
        }
    }

    // ------------------------------------------------------------
    // 부모 → 자식 직각 연결선
    // ------------------------------------------------------------
    private void DrawConnection(Node parent, Node child, int depth)
    {
        Rect p = nodeRects[parent];
        Rect c = nodeRects[child];

        float baseX = 20 + depth * indentWidth;
        float verticalX = baseX - 10;

        float parentY = p.center.y;
        float childY = c.center.y;

        Handles.color = Color.gray;

        Handles.DrawLine(
            new Vector2(baseX, parentY),
            new Vector2(verticalX, parentY)
        );

        Handles.DrawLine(
            new Vector2(verticalX, parentY),
            new Vector2(verticalX, childY)
        );

        Handles.DrawLine(
            new Vector2(verticalX, childY),
            new Vector2(baseX + indentWidth, childY)
        );
    }

    // ------------------------------------------------------------
    // 박스
    // ------------------------------------------------------------
    private void DrawNodeBox(Rect rect, Node node)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white }
        };

        GUI.Box(rect, node.GetType().Name, style);
    }

    // ------------------------------------------------------------
    // 아이콘 표시
    // ------------------------------------------------------------
    private void DrawNodeIcons(Rect rect, Node node)
    {
        float size = 16f;

        // 이번 프레임에 Update 안 된 노드는 아무 아이콘도 안 그림
        if (!controller.UpdatedThisFrame.Contains(node))
            return;

        if (!controller.LastResults.TryGetValue(node, out var state))
            return;

        // Running 아이콘
        if (state == NodeState.Running)
        {
            var r = new Rect(
                rect.xMax - size - 4,
                rect.y + (rect.height - size) * 0.5f,
                size, size);
            GUI.DrawTexture(r, iconRunning);
        }
        else
        {
            // Success / Failure 아이콘
            Texture2D tex = state == NodeState.Success ? iconSuccess : iconFailure;
            var r = new Rect(
                rect.xMax - size - 4,
                rect.y + (rect.height - size) * 0.5f,
                size, size);
            GUI.DrawTexture(r, tex);
        }
    }
}
