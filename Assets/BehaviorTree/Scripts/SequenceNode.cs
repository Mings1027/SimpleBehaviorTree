using BehaviorTree;

public class SequenceNode : CompositeNode
{
    public SequenceNode(params Node[] children)
    {
        if (children == null) return;
        this.children.AddRange(children);
    }

    public void AddChild(Node node) => children.Add(node);

    protected override NodeState OnUpdate()
    {
        foreach (var child in children)
        {
            var result = child.Update();
            if (result == NodeState.Success) continue;
            return result;
        }

        return NodeState.Success;
    }

    public override void DrawGizmos()
    {
        foreach (var child in children)
        {
            child.DrawGizmos();
        }
    }
}