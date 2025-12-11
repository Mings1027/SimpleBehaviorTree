using System.Collections.Generic;

namespace BehaviorTree
{
    public class SequenceNode : CompositeNode
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
                    case NodeState.Failure:
                        _currentIndex = 0; // 실패 시 전체 초기화
                        return NodeState.Failure;
                    case NodeState.Success:
                        _currentIndex++;
                        break;
                }
            }

            // 전체 성공 시 리셋
            _currentIndex = 0;
            return NodeState.Success;
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
