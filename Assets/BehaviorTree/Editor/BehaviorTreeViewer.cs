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

    // ──────────────────────────────────────────────────────────────
    //  depth → 세로선 최소Y / 최대Y 저장 (트리가 끊기지 않도록)
    // ──────────────────────────────────────────────────────────────
    private Dictionary<int, float> verticalMin = new();
    private Dictionary<int, float> verticalMax = new();

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
        verticalMin.Clear();
        verticalMax.Clear();

        DrawNodeRecursive(controller.RootNode, 0, true);

        DrawAllVerticalLines();
    }

    // =====================================================================
    // 재귀적으로 노드 그리기
    // =====================================================================
    private void DrawNodeRecursive(Node node, int depth, bool isLastChild)
    {
        float x = 20 + depth * indentWidth;
        Rect rect = new Rect(x, currentY, nodeWidth, nodeHeight);

        DrawNodeBox(rect, node);
        DrawNodeIcon(rect, node);

        // ───────────────────────────────
        // 세로선 범위 갱신
        // ───────────────────────────────
        if (!verticalMin.ContainsKey(depth))
            verticalMin[depth] = rect.center.y;
        verticalMax[depth] = rect.center.y;

        // ───────────────────────────────
        // 부모와 연결하는 가로선
        // ───────────────────────────────
        if (depth > 0)
        {
            Handles.color = Color.gray;
            Handles.DrawLine(
                new Vector2(rect.x - 10, rect.center.y),
                new Vector2(rect.x, rect.center.y)
            );
        }

        currentY += nodeHeight + verticalSpacing;

        // 자식 얻기
        var children = GetChildren(node);
        if (children == null) return;

        for (int i = 0; i < children.Count; i++)
        {
            bool childIsLast = (i == children.Count - 1);
            DrawNodeRecursive(children[i], depth + 1, childIsLast);
        }
    }


    // =====================================================================
    // 한 depth에 대해 ∣ 세로선 전체 그리기
    // =====================================================================
    private void DrawAllVerticalLines()
    {
        Handles.color = Color.gray;

        foreach (var kv in verticalMin)
        {
            int depth = kv.Key;
            float minY = kv.Value;
            float maxY = verticalMax[depth];

            float x = 20 + depth * indentWidth - 10;

            Handles.DrawLine(
                new Vector2(x, minY),
                new Vector2(x, maxY)
            );
        }
    }

    // =====================================================================
    // 노드 UI
    // =====================================================================
    private void DrawNodeBox(Rect rect, Node node)
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.MiddleLeft;
        style.normal.textColor = Color.white;
        GUI.Box(rect, node.GetType().Name, style);
    }

    private void DrawNodeIcon(Rect rect, Node node)
    {
        Texture2D icon = GetNodeIcon(node);
        if (icon == null) return;

        float size = 18f;

        Rect iconRect = new Rect(
            rect.xMax - size - 4,
            rect.y + (rect.height - size) * 0.5f,
            size, size
        );

        GUI.DrawTexture(iconRect, icon);
    }

    private Texture2D GetNodeIcon(Node node)
    {
        float now = Time.time;

        if (node.LastEvent == NodeEventType.Update &&
            node.LastEventFrame == Time.frameCount)
            return iconRunning;

        if (node.LastEvent == NodeEventType.Exit &&
            now - node.LastEventTime < 0.12f)
        {
            return node.LastResult switch
            {
                NodeState.Success => iconSuccess,
                NodeState.Failure => iconFailure,
                _ => null
            };
        }

        return null;
    }

    // =====================================================================
    // 자식 찾기
    // =====================================================================
    private List<Node> GetChildren(Node node)
    {
        if (node is SequenceNode seq)
            return (List<Node>)typeof(SequenceNode)
                .GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(seq);

        if (node is SelectorNode sel)
            return (List<Node>)typeof(SelectorNode)
                .GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(sel);

        if (node is ParallelNode par)
            return (List<Node>)typeof(ParallelNode)
                .GetField("_children", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(par);

        return null;
    }
}
