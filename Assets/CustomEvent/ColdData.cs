using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace CustomEvent
{
    internal class ColdData
    {
        [CanBeNull] internal Action<PooledEventData> Invoker;
        [CanBeNull] internal object InvokerCallback;
        [CanBeNull] internal object InvokeTarget;

        [CanBeNull] internal object InvokeObj;

        internal PooledEventData ManagedData;

        internal bool OneShot;

        internal void Clear()
        {
            Invoker = null;
            InvokerCallback = null;
            InvokeTarget = null;
            InvokeObj = null;
            ManagedData.target = null;
            OneShot = false;
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

            cold.InvokeTarget = target;
            cold.InvokerCallback = action;
            cold.Invoker = t =>
            {
                var callback = t.cold.InvokerCallback as Action<T>;
                Assert.IsNotNull(callback);
                
                var callbackTarget = t.cold.InvokeTarget as T;
                
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

            cold.InvokeTarget = target;
            cold.InvokerCallback = action;
            cold.Invoker = t =>
            {
                var callback = t.cold.InvokerCallback as Action<T, GameObject>;
                Assert.IsNotNull(callback);

                var callbackTarget = t.cold.InvokeTarget as T;
                var obj = t.cold.InvokeObj as GameObject;

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
