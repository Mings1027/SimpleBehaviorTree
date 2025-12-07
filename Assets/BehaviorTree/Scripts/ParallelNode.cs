using System.Collections.Generic;

namespace BehaviorTree
{
    public class ParallelNode : Node
    {
        private readonly List<Node> _children = new();
        private readonly ParallelPolicy _policy;

        public ParallelNode(ParallelPolicy policy, params Node[] children)
        {
            _policy = policy;
            if (children == null) return;
            _children.AddRange(children);
        }

        public ParallelNode(ParallelPolicy policy)
        {
            _policy = policy;
        }

        public void AddChild(Node node)
        {
            _children.Add(node);
        }

        protected override NodeState OnUpdate()
        {
            bool anySuccess = false;
            bool allSuccess = true;

            foreach (var child in _children)
            {
                var result = child.Update();

                if (_policy == ParallelPolicy.And)
                {
                    // AND 정책: 하나라도 Fail → Fail
                    if (result == NodeState.Failure)
                        return NodeState.Failure;

                    if (result != NodeState.Success)
                        allSuccess = false;
                }
                else
                {
                    // OR 정책: 하나라도 성공하면 성공
                    if (result == NodeState.Success)
                        anySuccess = true;
                }
            }

            // 정책별 최종 정리
            if (_policy == ParallelPolicy.And)
            {
                return allSuccess ? NodeState.Success : NodeState.Running;
            }

            if (_policy == ParallelPolicy.Or)
            {
                return anySuccess ? NodeState.Success : NodeState.Running;
            }

            return NodeState.Running;
        }
    }

    public enum ParallelPolicy
    {
        Or, // 하나라도 Success → Success
        And // 모든 노드가 Success → Success
    }
}