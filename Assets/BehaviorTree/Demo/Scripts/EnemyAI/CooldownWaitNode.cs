using BehaviorTree;

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
            // 쿨타임이 남아 있으면 계속 Running → 이동/공격 멈춤
            if (_ctx.attackCooldown.IsCoolingDown)
                return NodeState.Running;

            // 쿨타임이 끝나면 Sequence 성공 → 다시 MoveToTarget 또는 공격으로 진행
            return NodeState.Failure;
        }
    }
}