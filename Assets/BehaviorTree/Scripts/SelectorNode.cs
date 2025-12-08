using BehaviorTree;

public class SelectorNode : CompositeNode
{
    public SelectorNode(params Node[] children)
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
            if (result == NodeState.Failure) continue;
            return result;
        }

        return NodeState.Failure;
    }

    public override void DrawGizmos()
    {
        foreach (var child in children)
        {
            child.DrawGizmos();
        }
    }
}