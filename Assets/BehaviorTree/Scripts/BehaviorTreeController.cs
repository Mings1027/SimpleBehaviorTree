using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;

public class BehaviorTreeController : MonoBehaviour
{
    private Node _rootNode;
    public Node RootNode => _rootNode;

    void Update()
    {
        if (_rootNode == null)
            return;

#if UNITY_EDITOR
        ResetFlags(_rootNode);
#endif
        _rootNode.Update();
    }

    public void CreateTree(Node rootNode)
    {
        _rootNode = rootNode;
    }

    private void OnDrawGizmos()
    {
        _rootNode?.DrawGizmos();
    }

#if UNITY_EDITOR
    private void ResetFlags(Node node)
    {
        node.EnteredThisFrame = false;
        node.UpdatedThisFrame = false;
        node.ExitedThisFrame = false;

        var children = node.GetChildren();
        if (children == null)
            return;

        foreach (var child in children)
            ResetFlags(child);
    }
#endif
}