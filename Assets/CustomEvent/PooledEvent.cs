using System;
using UnityEngine;
using UnityEngine.Assertions;

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
            cold.OneShot = false;
            return new PooledEvent { cold = cold };
        }

        public PooledEvent Bind<T>(T target, Action<T> action) where T : class
        {
            Assert.IsTrue(IsValid,
                "PooledEvent must be created via PooledEvent.Create()" +
                "You cannot use 'new PooledEvent()'.");

            cold.ManagedData.Set(target, action);
            return this;
        }

        public PooledEvent Bind<T>(T target, Action<T, GameObject> action) where T : class
        {
            Assert.IsTrue(IsValid);
            ref var data = ref cold.ManagedData;
            data.Set(target, action);
            return this;
        }

        public PooledEvent OneShot()
        {
            Assert.IsTrue(IsValid, "PooledEvent.OneShot() called before Create().");
            cold.OneShot = true;
            return this;
        }
 
        public void Invoke()
        {
            Assert.IsTrue(IsValid,
                "PooledEvent.Invoke() called before PooledEvent was created. " +
                "Use PooledEvent.Create() to create it.");

            cold.Invoker?.Invoke(cold.ManagedData);

            if (!cold.OneShot) return;
            PooledEventManager.Instance.ReleaseCold(cold);
            cold = null; // 다시 호출해도 아무 일도 안 일어나도록
        }

        public void InvokeEvent(GameObject go)
        {
            Assert.IsTrue(IsValid);

            cold.InvokeObj = go;
            cold.Invoker?.Invoke(cold.ManagedData);

            if (!cold.OneShot) return;

            PooledEventManager.Instance.ReleaseCold(cold);
            cold = null;
        }

        public void Clear() => cold?.Clear();
    }
}
