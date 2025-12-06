namespace BehaviorTree
{
    public class FailNode : Node
    {
        protected override NodeState OnUpdate()
        {
            return NodeState.Failure;
        }
    }
}