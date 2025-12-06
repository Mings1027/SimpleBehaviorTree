using System.Collections.Generic;
using BehaviorTree;

public class SelectorNode : Node
{
    private readonly List<Node> _children = new();

    public SelectorNode(params Node[] children)
    {
        if (children == null) return;
        _children.AddRange(children);
    }

    public void AddChild(Node node) => _children.Add(node);

    protected override NodeState OnUpdate()
    {
        foreach (var child in _children)
        {
            var result = child.Update();
            if (result != NodeState.Failure)
                return result;
        }

        return NodeState.Failure;
    }

    public override void DrawGizmos()
    {
        foreach (var child in _children)
        {
            child.DrawGizmos();
        }
    }
}