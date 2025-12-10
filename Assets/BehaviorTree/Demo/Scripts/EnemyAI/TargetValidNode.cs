using UnityEngine;

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

            if (target == null) return NodeState.Failure;

            var distance = Vector3.Distance(target.position, _ctx.self.position);
            if (distance > _ctx.detectionRange)
            {
                _ctx.target = null;
                return NodeState.Failure;
            }

            return NodeState.Success;
        }
    }
}