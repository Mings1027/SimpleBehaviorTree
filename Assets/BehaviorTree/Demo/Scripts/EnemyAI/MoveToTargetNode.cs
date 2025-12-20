using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class MoveToTargetNode : Node
    {
        private readonly AIContext _ctx;

        private const float LOST_MULT = 1.2f;

        public MoveToTargetNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.self == null || _ctx.target == null)
                return NodeState.Failure;

            Vector3 selfPos = _ctx.self.position;
            Vector3 targetPos = _ctx.target.position;
            Vector3 toTarget = targetPos - selfPos;

            float sqrDist = toTarget.sqrMagnitude;
            float sqrAttackRange = _ctx.attackRange * _ctx.attackRange;
            float sqrLostRange = _ctx.detectionRange * _ctx.detectionRange * LOST_MULT * LOST_MULT;

            // 1) 타겟을 완전히 잃어버린 경우
            if (sqrDist > sqrLostRange)
            {
                _ctx.target = null;
                return NodeState.Failure;
            }

            // 2) 공격 사거리 안이면 이동 종료
            if (sqrDist <= sqrAttackRange)
                return NodeState.Success;

            // 3) 타겟 추적 + Separation 적용
            if (sqrDist > 0.0001f)
            {
                // 타겟 방향
                Vector3 moveDir = toTarget.normalized;

                // 가까운 적 회피 벡터
                Vector3 separation = GetSeparationForce();

                // 두 벡터 합성
                Vector3 finalDir = (moveDir + separation).normalized;

                _ctx.MoveDirection = finalDir;
            }

            return NodeState.Running;
        }


        private Vector3 GetSeparationForce()
        {
            Vector3 force = Vector3.zero;
            float desiredDistance = 2.0f; // 서로 최소 유지 거리
            float pushStrength = 2.0f; // 밀어내는 힘

            foreach (var other in EnemyManager.allEnemies)
            {
                if (other == null || other == _ctx.self) continue;

                float dist = Vector3.Distance(other.transform.position, _ctx.self.position);
                if (dist < desiredDistance && dist > 0.0001f)
                {
                    Vector3 pushDir = (_ctx.self.position - other.transform.position).normalized;
                    float scale = (desiredDistance - dist) / desiredDistance;

                    force += pushDir * (scale * pushStrength);
                }
            }

            return force;
        }
    }
}