using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class AttackNode : Node
    {
        private readonly AIContext _ctx;

        public AttackNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.self == null || _ctx.target == null)
                return NodeState.Failure;

            Debug.Log($"[Enemy Attack] {_ctx.self.name} -> {_ctx.target.name}");

            _ctx.attackCooldown.StartCooldown();
            return NodeState.Success;
        }
    }
}