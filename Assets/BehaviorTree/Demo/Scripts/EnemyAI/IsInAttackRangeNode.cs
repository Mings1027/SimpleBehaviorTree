using UnityEngine;

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

            var distance = Vector3.Distance(_ctx.self.position, _ctx.target.position);

            return distance <= _ctx.attackRange ? NodeState.Success : NodeState.Failure;
        }
    }
}