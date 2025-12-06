using BehaviorTree;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class CanAttackNode : Node
    {
        private readonly AIContext _ctx;

        public CanAttackNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            return _ctx.attackCooldown.IsCoolingDown
                ? NodeState.Failure
                : NodeState.Success;
        }
    }
}