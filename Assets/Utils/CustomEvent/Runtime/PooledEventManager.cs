using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomEvent
{
    [AddComponentMenu("")]
    internal class PooledEventManager : MonoBehaviour
    {
        internal static PooledEventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateInstance();
                }

                return _instance;
            }
        }

        private static PooledEventManager _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            _instance = null;
        }

#if UNITY_EDITOR
        private List<ColdData> _used; // 사용 중인 ColdData 추적
#endif
        private List<ColdData> _pool;
        private int _poolCapacity = 128;

        private static void CreateInstance()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                throw new InvalidOperationException("PooledEvent can only be used in Play Mode.");
#endif
            var go = new GameObject(nameof(PooledEventManager));
            _instance = go.AddComponent<PooledEventManager>();
            _instance.Init();
        }

        private void Init()
        {
            _pool = new List<ColdData>(_poolCapacity);
#if UNITY_EDITOR
            _used = new List<ColdData>(_poolCapacity);
#endif
            for (var i = 0; i < _poolCapacity; i++)
            {
                var cold = new ColdData();
                cold.ManagedData.cold = cold;
                _pool.Add(cold);
            }
        }

        internal static ColdData GetCold()
        {
            if (ReferenceEquals(Instance, null)) return null;

            var pool = _instance._pool;
            var poolCapacity = _instance._poolCapacity;

            if (pool.Count == 0)
            {
                // 자동 확장
                poolCapacity *= 2;
                for (var i = 0; i < poolCapacity; i++)
                    pool.Add(new ColdData());
            }

            var idx = pool.Count - 1;
            var cold = pool[idx];
            pool.RemoveAt(idx);
#if UNITY_EDITOR
            _instance._used.Add(cold); // 사용 중 목록에 추가 
#endif
            return cold;
        }

        internal static void ReleaseCold(ColdData cold)
        {
            if (ReferenceEquals(_instance, null))
            {
                cold.Clear();
                return;
            }

            cold.Clear();
            cold.ManagedData.cold = cold;
#if UNITY_EDITOR
            _instance._used.Remove(cold); // 사용 중 목록에서 제거 
#endif
            _instance._pool.Add(cold);
        }
    }
}