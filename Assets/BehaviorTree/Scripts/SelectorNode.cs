using System.Collections.Generic;

namespace BehaviorTree
{
    public class SelectorNode : CompositeNode
    {
        private int _currentIndex;

        public void AddChild(Node node)
        {
            children.Add(node);
        }

        protected override NodeState OnUpdate()
        {
            while (_currentIndex < children.Count)
            {
                var result = children[_currentIndex].Update();

                switch (result)
                {
                    case NodeState.Running:
                        return NodeState.Running;
                    case NodeState.Success:
                        _currentIndex = 0; // 성공 시 전체 리셋
                        return NodeState.Success;
                    case NodeState.Failure:
                        _currentIndex++;
                        break;
                }
            }

            _currentIndex = 0;
            return NodeState.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            _currentIndex = 0;
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Reset();
            }
        }
    }
}
