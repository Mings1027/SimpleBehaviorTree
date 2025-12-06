using BehaviorTree;
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
            return NodeState.Running;
        }
    }
}