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
        private bool IsValid => cold != null;

        public static PooledEvent Create()
        {
            var cold = PooledEventManager.GetCold();
            cold.ManagedData.cold = cold;
#if UNITY_EDITOR
            // 전역 스위치가 켜져있으면 로그 출력
            if (PooledEventManager.Instance.LogEnabled)
            {
                PrintLog("Created", null);
            }
#endif
            return new PooledEvent { cold = cold };
        }

        public void Dispose()
        {
            if (cold == null) return;

            PooledEventManager.ReleaseCold(cold);
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
            cold.DebugTargetObject = target as UnityEngine.Object;
            if (PooledEventManager.Instance.LogEnabled)
            {
                PrintLog("Bound", cold.DebugTargetObject);
            }
#endif
            return this;
        }

        public PooledEvent Bind<T>(T target, Action<T, GameObject> action) where T : class
        {
            AssertValid();
            cold.ResetForBind();
            cold.ManagedData.Set(target, action);
#if UNITY_EDITOR
            cold.DebugTargetObject = target as UnityEngine.Object;
            if (PooledEventManager.Instance.LogEnabled)
            {
                PrintLog("Bound", cold.DebugTargetObject);
            }
#endif
            return this;
        }

        public void Invoke()
        {
            if (cold == null) return;

            cold.Dispatch?.Invoke(cold.ManagedData);
#if UNITY_EDITOR
            if (cold.Dispatch != null) cold.DebugInvokeCount++;
#endif
        }

        public void Invoke(GameObject obj)
        {
            if (cold == null) return;
#if UNITY_EDITOR
            cold.DebugInvokeObj = obj;
#endif
            cold.DispatchObj?.Invoke(cold.ManagedData, obj);
#if UNITY_EDITOR
            if (cold.DispatchObj != null) cold.DebugInvokeCount++;
#endif
        }
        
#if UNITY_EDITOR
        private static void PrintLog(string action, UnityEngine.Object target)
        {
            // 호출 스택 추적 (0: PrintLog, 1: Create/Bind, 2: 사용자 코드)
            var st = new StackTrace(2, true);
            var frame = st.GetFrame(0);
            
            string location = "Unknown";
            if (frame != null)
            {
                string fileName = System.IO.Path.GetFileName(frame.GetFileName());
                int line = frame.GetFileLineNumber();
                location = $"{fileName}:{line}";
            }

            string targetName = target != null ? target.name : "No Target";
            string color = action == "Created" ? "cyan" : "green";

            Debug.Log($"<color={color}>[PooledEvent {action}]</color> Target: <b>{targetName}</b> / Location: <b>{location}</b>");
        }
#endif
    }
}