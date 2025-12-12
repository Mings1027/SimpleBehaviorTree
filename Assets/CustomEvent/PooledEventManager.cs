using System.Collections.Generic;
using UnityEngine;

namespace CustomEvent
{
    [AddComponentMenu("")]
    internal class PooledEventManager : MonoBehaviour
    {
        internal static PooledEventManager Instance;

        private List<ColdData> _pool;
        private int _poolCapacity = 128;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad()
        {
            if (Instance == null)
            {
                CreateInstance();
            }
        }

        private static void CreateInstance()
        {
            var go = new GameObject(nameof(PooledEventManager));
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PooledEventManager>();
            Instance.Init();
        }

        private void Init()
        {
            _pool = new List<ColdData>(_poolCapacity);
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
            return cold;
        }

        internal void ReleaseCold(ColdData cold)
        {
            cold.Clear();
            cold.ManagedData.cold = cold;

            _pool.Add(cold);
        }
    }
}
