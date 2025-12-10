using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class MoveToTargetNode : Node
    {
        private readonly AIContext _ctx;

        // 타겟을 완전히 놓쳤다고 판단하는 거리 배수
        // detectionRange * LOST_MULT 이상 멀어지면 타겟 포기
        private const float LOST_MULT = 1.2f;

        public MoveToTargetNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.self == null || _ctx.target == null)
                return NodeState.Failure;

            Vector3 selfPos   = _ctx.self.position;
            Vector3 targetPos = _ctx.target.position;
            Vector3 toTarget  = targetPos - selfPos;

            float sqrDist        = toTarget.sqrMagnitude;
            float sqrAttackRange = _ctx.attackRange   * _ctx.attackRange;
            float sqrLostRange   = _ctx.detectionRange * _ctx.detectionRange * LOST_MULT * LOST_MULT;

            // 1) 타겟을 완전히 잃어버린 경우만 포기
            if (sqrDist > sqrLostRange)
            {
                _ctx.target = null;
                return NodeState.Failure;
            }

            // 2) 공격 사거리 안이면 추적 완료 (공격 브랜치가 이어서 처리)
            if (sqrDist <= sqrAttackRange)
                return NodeState.Success;

            // 3) 그 외에는 계속 추적
            if (sqrDist > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;
                _ctx.self.position += dir * (_ctx.moveSpeed * Time.deltaTime);
            }

            return NodeState.Running;
        }
    }
}