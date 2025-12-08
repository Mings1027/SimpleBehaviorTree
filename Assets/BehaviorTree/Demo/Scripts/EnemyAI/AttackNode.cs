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

            // 실제 공격 로직 대신 로그만
            Debug.Log($"[Enemy Attack] {_ctx.self.name} -> {_ctx.target.name}");

            _ctx.attackCooldown.StartCooldown();
            return NodeState.Success;
        }
    }
}