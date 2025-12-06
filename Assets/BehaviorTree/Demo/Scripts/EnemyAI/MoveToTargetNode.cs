using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class MoveToTargetNode : Node
    {
        private readonly AIContext _ctx;

        public MoveToTargetNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.target == null)
                return NodeState.Failure;

            Vector3 toTarget = _ctx.target.position - _ctx.self.position;
            float sqrDist = toTarget.sqrMagnitude;
            float sqrAttackRange = _ctx.attackRange * _ctx.attackRange;

            // 공격 사거리 이내면 이동 완료
            if (sqrDist <= sqrAttackRange)
                return NodeState.Success;

            // 공격 사거리 밖이면 이동
            if (sqrDist > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;
                _ctx.self.position += dir * (_ctx.moveSpeed * Time.deltaTime);
            }

            return NodeState.Running;
        }
    }
}