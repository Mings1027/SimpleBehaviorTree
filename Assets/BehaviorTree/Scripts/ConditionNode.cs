using System;
using BehaviorTree;

public class ConditionNode : Node
{
    private readonly Func<bool> _condition;

    public ConditionNode(Func<bool> condition)
    {
        _condition = condition;
    }

    protected override NodeState OnUpdate()
    {
        return _condition() ? NodeState.Success : NodeState.Failure;
    }
}