namespace BehaviorTree
{
    public enum ParallelPolicy
    {
        Or, // 하나라도 Success → Success
        And // 모든 노드가 Success → Success
    }
}