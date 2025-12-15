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

        internal ColdData GetCold()
        {
            if (_pool.Count == 0)
            {
                // 자동 확장
                _poolCapacity *= 2;
                for (var i = 0; i < _poolCapacity; i++)
                    _pool.Add(new ColdData());
            }

            var idx = _pool.Count - 1;
            var cold = _pool[idx];
            _pool.RemoveAt(idx);
#if UNITY_EDITOR
            _used.Add(cold); // 사용 중 목록에 추가 
#endif
            return cold;
        }

        internal void ReleaseCold(ColdData cold)
        {
            cold.Clear();
            cold.ManagedData.cold = cold;
#if UNITY_EDITOR
            _used.Remove(cold); // 사용 중 목록에서 제거 
#endif
            _pool.Add(cold);
        }
    }
}