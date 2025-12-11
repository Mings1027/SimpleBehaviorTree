using System;
using UnityEngine.Assertions;

namespace PrimeEvent
{
    public struct CustomEvent
    {
        internal ColdData cold;

        // ColdData가 세팅된 상태인지 확인하는 내부 속성
        bool IsValid => cold != null;

        // ----------------------------
        // 지연 호출을 위한 생성
        // ----------------------------
        public static CustomEvent Create()
        {
            var cold = CustomEventManager.Instance.GetCold();
            cold.managedData.cold = cold;
            cold.oneShot = false;
            return new CustomEvent { cold = cold };
        }

        // ----------------------------
        // OnSet: default struct 에서 호출 시 Assert 에러
        // ----------------------------
        public CustomEvent OnSet<T>(T target, Action<T> action) where T : class
        {
            Assert.IsTrue(IsValid,
                "CustomEvent must be created via CustomEvent.Create() or CustomEvent.On(). " +
                "You cannot use 'new CustomEvent()'.");

            ref var data = ref cold.managedData;
            data.Set(target, action);
            return this;
        }

        public CustomEvent OneShot()
        {
            Assert.IsTrue(IsValid, "CustomEvent.OneShot() called before Create().");
            cold.oneShot = true;
            return this;
        }

        // ----------------------------
        // Invoke: default struct 에서 호출 시 Assert 에러
        // ----------------------------
        public void Invoke()
        {
            Assert.IsTrue(IsValid,
                "CustomEvent.Invoke() called before CustomEvent was created. " +
                "Use CustomEvent.Create() or CustomEvent.On() to create it.");

            cold.invoker?.Invoke(cold.data);
            
            if (!cold.oneShot) return;
            // 1회용이면 풀로 반환
            CustomEventManager.Instance.ReleaseCold(cold);
            cold = null; // 다시 호출해도 아무 일도 안 일어나도록
        }
    }
}