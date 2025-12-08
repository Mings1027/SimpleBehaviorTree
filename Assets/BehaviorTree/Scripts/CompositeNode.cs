using System.Collections.Generic;

namespace BehaviorTree
{
    public abstract class CompositeNode : Node
    {
        protected readonly List<Node> children = new();
        public IReadOnlyList<Node> Children => children;
    }
}