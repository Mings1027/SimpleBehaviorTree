namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class CooldownWaitNode : Node
    {
        private readonly AIContext _ctx;

        public CooldownWaitNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.attackCooldown.IsCoolingDown)
                return NodeState.Running;

            return NodeState.Success;
        }
    }
}