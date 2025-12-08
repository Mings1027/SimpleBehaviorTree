namespace BehaviorTree
{
    public class ParallelNode : CompositeNode
    {
        private readonly ParallelPolicy _policy;

        public ParallelNode(ParallelPolicy policy, params Node[] children)
        {
            _policy = policy;
            if (children != null)
                this.children.AddRange(children);
        }

        public ParallelNode(ParallelPolicy policy)
        {
            _policy = policy;
        }

        public void AddChild(Node node) => children.Add(node);

        protected override NodeState OnUpdate()
        {
            return _policy switch
            {
                ParallelPolicy.And => UpdateAnd(),
                ParallelPolicy.Or  => UpdateOr(),
                _ => NodeState.Running
            };
        }

        // -------------------------------------------------------------------
        // AND 정책 (원래 로직과 동일)
        // - 하나라도 Failure → Failure
        // - 모두 Success → Success
        // - 아니면 Running
        // -------------------------------------------------------------------
        private NodeState UpdateAnd()
        {
            bool allSuccess = true;

            foreach (var child in children)
            {
                var result = child.Update();

                if (result == NodeState.Failure)
                    return NodeState.Failure;

                if (result != NodeState.Success)
                    allSuccess = false;
            }

            return allSuccess ? NodeState.Success : NodeState.Running;
        }

        // -------------------------------------------------------------------
        // OR 정책 (원래 로직과 완전히 동일)
        // - 하나라도 Success → Success
        // - 아니면 Running (Failure 고려 X)
        // -------------------------------------------------------------------
        private NodeState UpdateOr()
        {
            foreach (var child in children)
            {
                var result = child.Update();

                if (result == NodeState.Success)
                    return NodeState.Success;
            }

            return NodeState.Running;
        }
    }

    public enum ParallelPolicy
    {
        Or,
        And
    }
}
