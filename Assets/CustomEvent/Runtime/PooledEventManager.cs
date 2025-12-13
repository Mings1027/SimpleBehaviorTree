using System.Collections.Generic;
using UnityEngine;

namespace CustomEvent
{
    [AddComponentMenu("")]
    internal class PooledEventManager : MonoBehaviour
    {
        internal static PooledEventManager Instance;
#if UNITY_EDITOR
        private List<ColdData> _used; // 사용 중인 ColdData 추적
#endif
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