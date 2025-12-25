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
        internal UnityEngine.Object DebugTargetObject;
        internal GameObject DebugInvokeObj;
        internal int DebugInvokeCount;
#endif

        internal void ResetForBind()
        {
            Callback = null;
            CallbackTarget = null;
            Dispatch = null;
            DispatchObj = null;

#if UNITY_EDITOR
            DebugInvokeObj = null;
            DebugInvokeCount = 0;
            // 주의: DebugLogEnabled는 여기서 초기화하지 않음 (Bind 될 때 설정 유지 여부는 선택)
            // 보통은 재사용될 때 꺼지는 게 안전하므로 Clear에서 끕니다.
#endif
        }

        internal void Clear()
        {
            ResetForBind();
            ManagedData.target = null;

#if UNITY_EDITOR
            DebugTargetObject = null;
#endif
        }
    }

    internal struct PooledEventData
    {
        internal object target;
        internal ColdData cold;

        internal void Set<T>([CanBeNull] T target, [CanBeNull] Action<T> action) where T : class
        {
            if (action == null) return;

            Assert.IsNotNull(cold, "PooledEventData.cold is null. Create() / ReleaseCold()에서 cold 연결을 확인하세요.");

            cold.DispatchObj = null;

            cold.CallbackTarget = target;
            cold.Callback = action;
            cold.Dispatch = t =>
            {
                var callback = t.cold.Callback as Action<T>;
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

            cold.Dispatch = null;

            cold.CallbackTarget = target;
            cold.Callback = action;
            cold.DispatchObj = (t, obj) =>
            {
                var callback = t.cold.Callback as Action<T, GameObject>;
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