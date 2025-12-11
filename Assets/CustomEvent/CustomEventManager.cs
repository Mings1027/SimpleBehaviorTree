using System.Collections.Generic;
using UnityEngine;

namespace PrimeEvent
{
    [AddComponentMenu("")]
    internal sealed class CustomEventManager : MonoBehaviour
    {
        internal static CustomEventManager Instance;

        List<ColdData> pool;
        int poolCapacity = 128;

        // -----------------------------
        // 1) 자동 생성
        // -----------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            if (Instance == null)
            {
                CreateInstance();
            }
        }

        static void CreateInstance()
        {
            var go = new GameObject(nameof(CustomEventManager));
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<CustomEventManager>();
            Instance.Init();
        }

        void Init()
        {
            pool = new List<ColdData>(poolCapacity);
            for (int i = 0; i < poolCapacity; i++)
                pool.Add(new ColdData());
        }

        // -----------------------------
        // 2) ColdData 가져오기
        // -----------------------------
        internal ColdData GetCold()
        {
            if (pool.Count == 0)
            {
                // 자동 확장
                poolCapacity *= 2;
                for (int i = 0; i < poolCapacity; i++)
                    pool.Add(new ColdData());
            }

            var idx = pool.Count - 1;
            var cold = pool[idx];
            pool.RemoveAt(idx);
            return cold;
        }

        // -----------------------------
        // 3) 반환하기
        // -----------------------------
        internal void ReleaseCold(ColdData cold)
        {
            cold.invoker = null;
            cold.invokerCallback = null;
            cold.invokeTarget = null;
            cold.managedData = default;

            pool.Add(cold);
        }
    }
 
}