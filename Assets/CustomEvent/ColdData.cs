using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrimeEvent
{
    internal partial class ColdData
    {
        [CanBeNull] internal Action<CustomEventData> invoker;
        [CanBeNull] internal object invokerCallback;
        [CanBeNull] internal object invokeTarget;

        internal ref CustomEventData data => ref managedData;
        internal CustomEventData managedData;

        internal bool oneShot;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal struct CustomEventData
    {
        [FieldOffset(0)] internal object target;
        [FieldOffset(8)] internal ColdData cold;
        
        internal void Set<T>([CanBeNull] T _target, [CanBeNull] Action<T> action) where T : class
        {
            if (action == null) return;

            cold.invokeTarget = _target;
            cold.invokerCallback = action;
            cold.invoker = t =>
            {
                var callback = t.cold.invokerCallback as Action<T>;
                Assert.IsNotNull(callback);
                var callbackTarget = t.cold.invokeTarget as T;
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

        void HandleCallbackException(Exception e)
        {
            Debug.LogException(e, target as UnityEngine.Object);
        }
    }
}