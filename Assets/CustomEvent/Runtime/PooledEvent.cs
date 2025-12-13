using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace CustomEvent
{
    public struct PooledEvent
    {
        internal ColdData cold;

        // ColdData가 세팅된 상태인지 확인하는 내부 속성
        private bool IsValid => cold != null;

        public static PooledEvent Create()
        {
            var cold = PooledEventManager.Instance.GetCold();
            cold.ManagedData.cold = cold;
            return new PooledEvent { cold = cold };
        }

        public PooledEvent Bind<T>(T target, Action<T> action) where T : class
        {
            Assert.IsTrue(IsValid,
                "PooledEvent must be created via PooledEvent.Create()" +
                "You cannot use 'new PooledEvent()'.");

            cold.ManagedData.Set(target, action);

#if UNITY_EDITOR
            cold.DebugTargetObject = target as UnityEngine.Object;
            cold.DebugInvokeCount = 0;

            var st = new StackTrace(true);
            var frame = st.GetFrame(1); // Bind를 호출한 쪽
            if (frame != null)
            {
                cold.DebugBindFile = frame.GetFileName();
                cold.DebugBindLine = frame.GetFileLineNumber();
            }
#endif
            return this;
        }

        public PooledEvent Bind<T>(T target, Action<T, GameObject> action) where T : class
        {
            Assert.IsTrue(IsValid);
            ref var data = ref cold.ManagedData;
            data.Set(target, action);

#if UNITY_EDITOR
            cold.DebugTargetObject = target as UnityEngine.Object;
            cold.DebugInvokeCount = 0;

            var st = new StackTrace(true);
            var frame = st.GetFrame(1); // Bind를 호출한 쪽
            if (frame != null)
            {
                cold.DebugBindFile = frame.GetFileName();
                cold.DebugBindLine = frame.GetFileLineNumber();
            }
#endif
            return this;
        }

        public void Invoke()
        {
            Assert.IsTrue(IsValid,
                "PooledEvent.Invoke() called before PooledEvent was created. " +
                "Use PooledEvent.Create() to create it.");

            cold.Dispatch?.Invoke(cold.ManagedData);

#if UNITY_EDITOR
            if (cold.Dispatch != null)
                cold.DebugInvokeCount++;
#endif
        }

        public void InvokeEvent(GameObject go)
        {
            Assert.IsTrue(IsValid);

#if UNITY_EDITOR
            cold.DebugInvokeObj = go;
#endif

            cold.DispatchObj?.Invoke(cold.ManagedData, go);

#if UNITY_EDITOR
            if (cold.DispatchObj != null)
                cold.DebugInvokeCount++;
#endif
        }

        public void Dispose()
        {
            if (cold == null) return;
            PooledEventManager.Instance.ReleaseCold(cold);
            cold = null;
        }
 
    }
}