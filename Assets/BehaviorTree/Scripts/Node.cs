using UnityEngine;

namespace BehaviorTree
{
    public abstract class Node
    {
        private bool _started = false;

        public NodeEventType LastEvent { get; private set; } = NodeEventType.None;
        public int LastEventFrame { get; private set; }
        public float LastEventTime { get; private set; }

        public NodeState LastResult { get; private set; } = NodeState.Running;

        public NodeState Update()
        {
            if (!_started)
            {
                MarkEnter();
                OnStart();
                _started = true;
            }

            MarkUpdate();
            NodeState result = OnUpdate();
            LastResult = result;

            if (result != NodeState.Running)
            {
                MarkExit();
                OnEnd();
                _started = false;
            }

            return result;
        }

        protected virtual void OnStart() { }
        protected abstract NodeState OnUpdate();
        protected virtual void OnEnd() { }
        public virtual void DrawGizmos() { }

        protected void MarkEnter()
        {
            LastEvent = NodeEventType.Enter;
            LastEventFrame = Time.frameCount;
            LastEventTime = Time.time;
        }

        protected void MarkUpdate()
        {
            LastEvent = NodeEventType.Update;
            LastEventFrame = Time.frameCount;
            LastEventTime = Time.time;
        }

        protected void MarkExit()
        {
            LastEvent = NodeEventType.Exit;
            LastEventFrame = Time.frameCount;
            LastEventTime = Time.time;
        }
    }

    public enum NodeEventType
    {
        None,
        Enter,
        Update,
        Exit
    }
}