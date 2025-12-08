namespace BehaviorTree
{
    public abstract class Node
    {
        private bool _started;

#if UNITY_EDITOR
        // 노드 결과 (Success / Failure / Running)
        public NodeState LastResult { get; private set; } = NodeState.Running;

        public bool EnteredThisFrame { get; internal set; }
        public bool UpdatedThisFrame { get; internal set; }
        public bool ExitedThisFrame { get; internal set; }
#endif
        public NodeState Update()
        {
            // Enter 처리
            if (!_started)
            {
#if UNITY_EDITOR
                MarkEnter();
#endif
                OnStart();
                _started = true;
            }

            // Update 처리
#if UNITY_EDITOR
            MarkUpdate();
#endif
            NodeState result = OnUpdate();
#if UNITY_EDITOR
            LastResult = result;
#endif

            // Exit 처리
            if (result != NodeState.Running)
            {
#if UNITY_EDITOR
                MarkExit();
#endif
                OnEnd();
                _started = false;
            }

            return result;
        }

        protected virtual void OnStart() { }
        protected abstract NodeState OnUpdate();
        protected virtual void OnEnd() { }
        public virtual void DrawGizmos() { }

        // ==========================================================
        // ★ 이벤트 기록 함수들 (Enter / Update / Exit)
        // ==========================================================
#if UNITY_EDITOR
        private void MarkEnter() => EnteredThisFrame = true; // ← Viewer 에서 Enter 아이콘 표시 가능
        private void MarkUpdate() => UpdatedThisFrame = true; // ← Running 아이콘 표시 가능
        private void MarkExit() => ExitedThisFrame = true; // ← Success/Failure 아이콘 표시 가능
#endif
    }

    public enum NodeState
    {
        Success,
        Failure,
        Running
    }
}