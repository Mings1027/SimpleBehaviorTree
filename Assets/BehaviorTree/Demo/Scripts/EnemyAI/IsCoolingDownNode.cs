namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class IsCoolingDownNode : Node
    {
        private readonly AIContext _ctx;

        public IsCoolingDownNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            return _ctx.attackCooldown.IsCoolingDown ? NodeState.Success : NodeState.Failure;
        }
    }
}