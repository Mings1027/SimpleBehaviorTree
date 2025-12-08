namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class IsInAttackRangeNode : Node
    {
        private readonly AIContext _ctx;

        public IsInAttackRangeNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.self == null || _ctx.target == null)
                return NodeState.Failure;

            float sqrDist = (_ctx.target.position - _ctx.self.position).sqrMagnitude;
            float sqrRange = _ctx.attackRange * _ctx.attackRange;

            return sqrDist <= sqrRange ? NodeState.Success : NodeState.Failure;
        }
    }
}