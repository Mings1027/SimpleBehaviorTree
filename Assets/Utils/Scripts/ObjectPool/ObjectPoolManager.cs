using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolPreset
{
    public GameObject Prefab;
    public int Count;
    public bool UseAutoRelease;
    public float AutoReleaseDelay;
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private class Pool
    {
        public readonly Stack<GameObject> Stack = new();
        public readonly HashSet<GameObject> Set = new();
    }

    private struct PoolConfig
    {
        public bool UseAutoRelease;
        public float AutoReleaseDelay;
    }

    private static readonly Dictionary<GameObject, Pool> Pools = new();
    private static readonly Dictionary<GameObject, PoolConfig> PoolConfigs = new();

    [Header("Prewarm Presets")]
    [SerializeField] private List<PoolPreset> presets = new();

    protected override void Awake()
    {
        PrewarmFromPresets();
    }

    private void PrewarmFromPresets()
    {
        foreach (var preset in presets)
        {
            if (preset.Prefab == null || preset.Count <= 0)
                continue;

            Register(
                preset.Prefab,
                preset.Count,
                preset.UseAutoRelease,
                preset.AutoReleaseDelay
            );
        }
    }

    public static void Register(GameObject prefab, int preloadCount = 0, bool useAutoRelease = false,
        float autoReleaseDelay = 0f)
    {
        _ = Instance;
        if (Pools.ContainsKey(prefab))
            return;

        Pools[prefab] = new Pool();
        PoolConfigs[prefab] = new PoolConfig
        {
            UseAutoRelease = useAutoRelease,
            AutoReleaseDelay = autoReleaseDelay
        };

        for (int i = 0; i < preloadCount; i++)
        {
            var instance = CreateInstance(prefab);
            instance.SetActive(false);
            InternalRelease(prefab, instance);
        }
    }

    private static GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        instance.name = prefab.name;

        if (PoolConfigs.TryGetValue(prefab, out var config) &&
            config.UseAutoRelease)
        {
            if (!instance.TryGetComponent(out AutoReleaseToPool auto))
                auto = instance.AddComponent<AutoReleaseToPool>();

            auto.Init(prefab, config.AutoReleaseDelay);
        }

        return instance;
    }

    public static GameObject Get(GameObject prefab, bool setActive = true)
    {
        if (!Pools.TryGetValue(prefab, out var pool))
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"[ObjectPoolManager] Prefab '{prefab.name}' was not registered."
            );
#endif
            Register(prefab);
            pool = Pools[prefab];
        }

        GameObject obj = pool.Stack.Count > 0
            ? pool.Stack.Pop()
            : CreateInstance(prefab);

        pool.Set.Remove(obj);

        if (obj.TryGetComponent(out AutoReleaseToPool auto))
            auto.OnGet();

        obj.SetActive(setActive);
        return obj;
    }

    public static void Release(GameObject prefab, GameObject instance)
    {
        if (!Pools.TryGetValue(prefab, out _))
            return;

        if (instance.TryGetComponent(out AutoReleaseToPool auto))
            auto.OnRelease();

        instance.SetActive(false);

        InternalRelease(prefab, instance);
    }

    public static void ClearPool(GameObject prefab, bool removePool = false)
    {
        if (!Pools.TryGetValue(prefab, out var pool))
            return;

        // 인스턴스 전부 제거
        foreach (var obj in pool.Set)
        {
            if (obj != null)
                Destroy(obj);
        }

        pool.Stack.Clear();
        pool.Set.Clear();

        // 풀 자체 제거 옵션
        if (removePool)
        {
            Pools.Remove(prefab);
            PoolConfigs.Remove(prefab);
        }
    }


    private static void InternalRelease(GameObject prefab, GameObject instance)
    {
        var pool = Pools[prefab];

        // HashSet.Add → false면 이미 풀에 있음
        if (!pool.Set.Add(instance))
            return;

        pool.Stack.Push(instance);
    }
}