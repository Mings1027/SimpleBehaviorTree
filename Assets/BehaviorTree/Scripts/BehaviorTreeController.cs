using UnityEngine;
using BehaviorTree;

public class BehaviorTreeController : MonoBehaviour
{
    private Node _rootNode;
#if UNITY_EDITOR
    public Node RootNode => _rootNode;
#endif

    private void Update()
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        _rootNode?.DrawGizmos();
    }

    private static void ResetFlags(Node node)
    {
        node.EnteredThisFrame = false;
        node.UpdatedThisFrame = false;
        node.ExitedThisFrame = false;

        if (node is CompositeNode comp)
        {
            for (var i = 0; i < comp.Children.Count; i++)
            {
                var child = comp.Children[i];
                ResetFlags(child);
            }
        }
    }
#endif
}