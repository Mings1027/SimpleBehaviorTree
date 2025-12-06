using BehaviorTree;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class HasTargetNode : Node
    {
        private readonly AIContext _ctx;

        public HasTargetNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            return _ctx.target != null ? NodeState.Success : NodeState.Failure;
        }
    }
}