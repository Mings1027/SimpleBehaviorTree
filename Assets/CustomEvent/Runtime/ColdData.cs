using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace CustomEvent
{
    internal class ColdData
    {
        [CanBeNull] internal object Callback;

        [CanBeNull] internal object CallbackTarget;

        [CanBeNull] internal Action<PooledEventData> Dispatch;
        [CanBeNull] internal Action<PooledEventData, GameObject> DispatchObj;

        internal PooledEventData ManagedData;

#if UNITY_EDITOR
        internal UnityEngine.Object DebugTargetObject; // 어떤 객체가 바인딩됐는지
        internal GameObject DebugInvokeObj; // InvokeEvent로 전달된 GameObject
        internal string DebugBindFile; // 어디 파일 Bind됐는지
        internal int DebugBindLine; // 몇 번째 줄에서 Bind됐는지
        internal int DebugInvokeCount; // 몇 번 Invoke됐는지
#endif
        internal void Clear()
        {
            Dispatch = null;
            DispatchObj = null;
            Callback = null;
            CallbackTarget = null;
            // CallbackObj = null;
            ManagedData.target = null;

#if UNITY_EDITOR
            DebugTargetObject = null;
            DebugInvokeObj = null;
            DebugBindFile = null;
            DebugBindLine = 0;
            DebugInvokeCount = 0;
#endif
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal struct PooledEventData
    {
        [FieldOffset(0)] internal object target;
        [FieldOffset(8)] internal ColdData cold;

        internal void Set<T>([CanBeNull] T target, [CanBeNull] Action<T> action) where T : class
        {
            if (action == null) return;

            Assert.IsNotNull(cold, "PooledEventData.cold is null. Create() / ReleaseCold()에서 cold 연결을 확인하세요.");

            cold.CallbackTarget = target;
            cold.Callback = action;
            cold.Dispatch = t =>
            {
                var callback = t.cold.Callback as Action<T>;
                Assert.IsNotNull(callback);

                var callbackTarget = t.cold.CallbackTarget as T;

                try
                {
                    callback(callbackTarget);
                }
                catch (Exception e)
                {
                    t.HandleCallbackException(e);
                }
            };
        }

        internal void Set<T>([CanBeNull] T target, Action<T, GameObject> action) where T : class
        {
            if (action == null) return;
            Assert.IsNotNull(cold, "PooledEventData.cold is null. Create() / ReleaseCold()에서 cold 연결을 확인하세요.");

            cold.CallbackTarget = target;
            cold.Callback = action;
            cold.DispatchObj = (t, obj) =>
            {
                var callback = t.cold.Callback as Action<T, GameObject>;
                Assert.IsNotNull(callback);

                var callbackTarget = t.cold.CallbackTarget as T;

                try
                {
                    callback(callbackTarget, obj);
                }
                catch (Exception e)
                {
                    t.HandleCallbackException(e);
                }
            };
        }

        void HandleCallbackException(Exception e)
        {
            Debug.LogException(e, target as UnityEngine.Object);
        }
    }
}