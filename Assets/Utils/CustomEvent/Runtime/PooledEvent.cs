using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace CustomEvent
{
    public struct PooledEvent : IDisposable
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

        public void Dispose()
        {
            if (cold == null) return;
            PooledEventManager.Instance.ReleaseCold(cold);
            cold = null;
        }

        private void AssertValid()
        {
            Assert.IsTrue(IsValid,
                "PooledEvent is invalid. " +
                "It may have been disposed or not created via Create().");
        }

        public PooledEvent Bind<T>(T target, Action<T> action) where T : class
        {
            AssertValid();
            cold.ResetForBind();
            cold.ManagedData.Set(target, action);
#if UNITY_EDITOR
            SetupDebug(target);
#endif
            return this;
        }

        public PooledEvent Bind<T>(T target, Action<T, GameObject> action) where T : class
        {
            AssertValid();
            cold.ResetForBind();
            cold.ManagedData.Set(target, action);
#if UNITY_EDITOR
            SetupDebug(target);
#endif
            return this;
        }

        public void Invoke()
        {
            if (cold == null) return;
            cold.Dispatch?.Invoke(cold.ManagedData);
#if UNITY_EDITOR
            if (cold.Dispatch != null)
                cold.DebugInvokeCount++;
#endif
        }

        public void InvokeEvent(GameObject go)
        {
            if (cold == null) return;
#if UNITY_EDITOR
            cold.DebugInvokeObj = go;
#endif
            cold.DispatchObj?.Invoke(cold.ManagedData, go);
#if UNITY_EDITOR
            if (cold.DispatchObj != null)
                cold.DebugInvokeCount++;
#endif
        }

#if UNITY_EDITOR
        private void SetupDebug(object target)
        {
            cold.DebugTargetObject = target as UnityEngine.Object;
            cold.DebugInvokeCount = 0;

            var st = new StackTrace(true);
            var frame = st.GetFrame(2);
            if (frame != null)
            {
                cold.DebugBindFile = frame.GetFileName();
                cold.DebugBindLine = frame.GetFileLineNumber();
            }
        }
#endif
    }
}