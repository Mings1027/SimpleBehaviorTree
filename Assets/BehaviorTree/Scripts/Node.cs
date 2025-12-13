using System;

namespace BehaviorTree
{
    public abstract class Node
    {
        private bool _started;

        // 이 이벤트만으로 누가 언제 Update됐는지 알 수 있음
        public static event Action<Node, NodeState> OnNodeUpdated;

        public NodeState Update()
        {
            if (!_started)
            {
                OnStart();
                _started = true;
            }

            NodeState result = OnUpdate();

            // 컨트롤러/뷰어는 이 이벤트만 구독하면 됨
            OnNodeUpdated?.Invoke(this, result);

            if (result != NodeState.Running)
            {
                OnEnd();
                _started = false;
            }

            return result;
        }

        protected virtual void OnStart() { }
        protected abstract NodeState OnUpdate();
        protected virtual void OnEnd() { }
        public virtual void DrawGizmos() { }

        public virtual void Reset() { }
    }

    public enum NodeState
    {
        Success,
        Failure,
        Running
    }
}