using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class MoveToWaitPositionNode : Node
    {
        private readonly AIContext _ctx;

        public MoveToWaitPositionNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            if (_ctx.self == null || _ctx.waitPosition == null)
                return NodeState.Failure;

            // 이동 중에 타겟이 생기면 즉시 실패 → 상위 Selector 가 전투 루프로 전환
            if (_ctx.target != null)
                return NodeState.Failure;

            Vector3 toWait = _ctx.waitPosition.position - _ctx.self.position;
            float sqrDist = toWait.sqrMagnitude;

            if (sqrDist <= 0.01f)
            {
                return NodeState.Success; // 대기 위치 도착
            }

            Vector3 dir = toWait.normalized;
            _ctx.self.position += dir * (_ctx.moveSpeed * Time.deltaTime);

            return NodeState.Running;
        }
    }
}