using System;
using BehaviorTree;

public class ActionNode : Node
{
    private readonly Func<NodeState> _action;

    public ActionNode(Func<NodeState> action)
    {
        _action = action;
    }

    protected override NodeState OnUpdate()
    {
        return _action();
    }
}