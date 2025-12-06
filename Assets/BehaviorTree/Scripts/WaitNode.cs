using BehaviorTree;
using UnityEngine;

public class WaitNode : Node
{
    private float waitTime;
    private float startTime;

    public WaitNode(float t)
    {
        waitTime = t;
    }

    protected override void OnStart()
    {
        startTime = Time.time;
        Debug.Log($"{GetType().Name} Start");
    }

    protected override NodeState OnUpdate()
    {
        if (Time.time - startTime < waitTime)
            return NodeState.Running;

        return NodeState.Success;
    }

    protected override void OnEnd()
    {
        Debug.Log($"{GetType().Name} End");
    }
}