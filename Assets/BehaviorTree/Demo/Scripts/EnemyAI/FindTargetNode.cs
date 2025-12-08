using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    public class FindTargetNode : Node
    {
        private readonly AIContext _ctx;

        public FindTargetNode(AIContext ctx)
        {
            _ctx = ctx;
        }

        protected override NodeState OnUpdate()
        {
            // 1. 이미 타겟이 있으면 그대로 유지 (여기선 절대 안 바꿈)
            if (_ctx.target != null)
            {
                // 만약 죽었거나 비활성화되면 여기서만 지워주고 새로 찾기
                if (!_ctx.target.gameObject.activeSelf)
                {
                    _ctx.target = null;
                }
                else
                {
                    return NodeState.Success;
                }
            }

            int count = Physics.OverlapSphereNonAlloc(
                _ctx.self.position,
                _ctx.detectionRange,
                _ctx.targetBuffer,
                _ctx.targetMask
            );

            float best = float.MaxValue;
            Transform bestTarget = null;

            for (int i = 0; i < count; i++)
            {
                var col = _ctx.targetBuffer[i];
                if (col == null) continue;

                var t = col.transform;
                if (t == _ctx.self) continue;

                float sqr = (t.position - _ctx.self.position).sqrMagnitude;
                if (sqr < best)
                {
                    best = sqr;
                    bestTarget = t;
                }
            }

            _ctx.target = bestTarget;
            return NodeState.Success;
        }
    }
}