namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class TargetValidNode : Node
    {
        private readonly AIContext _ctx;

        public TargetValidNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            var target = _ctx.target;

            // 타겟 없음 → Combat 불가
            if (target == null)
                return NodeState.Failure;

            // 타겟이 너무 멀어짐 → Combat 종료
            float distSqr = (target.position - _ctx.self.position).sqrMagnitude;
            if (distSqr > _ctx.detectionRange * _ctx.detectionRange)
            {
                _ctx.target = null; // ← 타겟 제거!
                return NodeState.Failure;
            }

            return NodeState.Success;
        }
    }
}