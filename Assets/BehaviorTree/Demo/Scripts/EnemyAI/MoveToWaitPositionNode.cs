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
            // 타겟 생기면 WaitParallel 종료 → Combat으로 이동
            if (_ctx.target != null)
                return NodeState.Failure;

            Vector3 targetPos = _ctx.waitPosition.position;
            float dist = Vector3.Distance(_ctx.self.position, targetPos);

            // 이동 중
            if (dist > 0.2f)
            {
                _ctx.MoveDirection = (targetPos - _ctx.self.position).normalized;
                return NodeState.Running;
            }

            // 도착했어도 계속 Running → Idle 유지 상태
            _ctx.MoveDirection = Vector3.zero;
            return NodeState.Success;
        }
    }
}