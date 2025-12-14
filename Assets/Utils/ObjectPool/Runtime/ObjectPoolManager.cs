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
        public readonly Stack<GameObject> Inactive = new();
        public int Total;
    }

    private struct PoolConfig
    {
        public bool UseAutoRelease;
        public float AutoReleaseDelay;
    }

    private static readonly Dictionary<GameObject, Pool> Pools = new();
    private static readonly Dictionary<GameObject, PoolConfig> PoolConfigs = new();
    private static readonly List<AutoReleaseToPool> AutoReleaseList = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnSubsystemRegistration()
    {
        _instance = null;

        Pools.Clear();
        PoolConfigs.Clear();
        AutoReleaseList.Clear();
    }

    [Header("Pool Presets")]
    [SerializeField] private List<PoolPreset> presets = new();

    protected override void Awake()
    {
        PrewarmFromPresets();
    }

    private void Update()
    {
        float now = Time.time;

        for (int i = AutoReleaseList.Count - 1; i >= 0; i--)
        {
            var auto = AutoReleaseList[i];

            if (auto == null)
            {
                AutoReleaseList.RemoveAt(i);
                continue;
            }

            if (now < auto.ReleaseAt)
                continue;

            auto.CancelTimer();
            auto.Release();
            DeregisterAutoRelease(auto);
        }
    }

    /// <summary>
    /// Inspector에 설정된 PoolPreset 목록 기반으로 각 프리팹 풀을 등록
    /// </summary>
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

    /// <summary>
    /// 프리팹을 풀에 등록하고 필요 시 지정개수만큼 미리 생성
    /// </summary>
    /// <param name="prefab">프리팹</param>
    /// <param name="preloadCount">생성해 둘 오브젝트 개수</param>
    /// <param name="useAutoRelease">자동 반환 사용 여부</param>
    /// <param name="autoReleaseDelay">자동 반환 대기 시간</param>
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

    /// <summary>
    /// 프리팹 기반으로 오브젝트 인스턴스 생성
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    private static GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        instance.name = prefab.name;

        if (!instance.TryGetComponent(out PooledObject pooled))
            pooled = instance.AddComponent<PooledObject>();

        pooled.Init(prefab);

        var pool = Pools[prefab];
        pool.Total++;

#if UNITY_EDITOR
        var root = GetOrCreatePoolRoot(prefab);
        instance.transform.SetParent(root, false);
#endif

        if (PoolConfigs.TryGetValue(prefab, out var config) &&
            config.UseAutoRelease)
        {
            if (!instance.TryGetComponent(out AutoReleaseToPool auto))
                auto = instance.AddComponent<AutoReleaseToPool>();

            auto.Init(pooled, config.AutoReleaseDelay);
        }

        return instance;
    }

    /// <summary>
    /// 풀에서 오브젝트 하나를 가져옴
    /// 없으면 새로 생성됨
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="setActive"></param>
    /// <returns></returns>
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

        GameObject obj = pool.Inactive.Count > 0
            ? pool.Inactive.Pop()
            : CreateInstance(prefab);

        if (obj.TryGetComponent(out PooledObject pooled))
            pooled.MarkInUse();

        if (obj.TryGetComponent(out AutoReleaseToPool auto))
        {
            auto.StartTimer();
            RegisterAutoRelease(auto);
        }

        obj.SetActive(setActive);
        return obj;
    }

    /// <summary>
    /// 풀에 반환하는 함수
    /// 이미 풀에 있거나 유효하지 않은 경우 무시
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="instance"></param>
    public static void Release(GameObject prefab, GameObject instance)
    {
        if (!Pools.TryGetValue(prefab, out _)) return;

        if (!instance.TryGetComponent(out PooledObject pooled)) return;

        if (pooled.IsInPool) return;

        if (instance.TryGetComponent(out AutoReleaseToPool auto))
        {
            auto.CancelTimer();
            DeregisterAutoRelease(auto);
        }

        instance.SetActive(false);

        pooled.MarkReleased();

        InternalRelease(prefab, instance);
    }

    /// <summary>
    /// 특정 프리팹 풀을 비움
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="removePool"></param>
    public static void ClearPool(GameObject prefab, bool removePool = false)
    {
        if (!Pools.TryGetValue(prefab, out var pool)) return;

        while (pool.Inactive.Count > 0)
        {
            var obj = pool.Inactive.Pop();
            if (obj != null)
                Destroy(obj);
        }

        pool.Total = 0;

#if UNITY_EDITOR
        if (PoolRoots.TryGetValue(prefab, out var root) && root != null)
        {
            DestroyImmediate(root.gameObject);
            PoolRoots.Remove(prefab);
        }
#endif

        if (removePool)
        {
            Pools.Remove(prefab);
            PoolConfigs.Remove(prefab);
        }
    }

    /// <summary>
    /// 오브젝트를 내부 스택에 반환
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="instance"></param>
    private static void InternalRelease(GameObject prefab, GameObject instance)
    {
#if UNITY_EDITOR
        var root = GetOrCreatePoolRoot(prefab);
        instance.transform.SetParent(root, false);
#endif
        var pool = Pools[prefab];
        pool.Inactive.Push(instance);
    }

    internal static void RegisterAutoRelease(AutoReleaseToPool auto)
    {
        if (!AutoReleaseList.Contains(auto))
            AutoReleaseList.Add(auto);
    }

    internal static void DeregisterAutoRelease(AutoReleaseToPool auto)
    {
        AutoReleaseList.Remove(auto);
    }

#if UNITY_EDITOR
    // 디버그 조회용
    public static IReadOnlyDictionary<GameObject, (int total, int inactive)> GetPoolDebugInfo()
    {
        var result = new Dictionary<GameObject, (int, int)>();
        foreach (var kv in Pools)
            result[kv.Key] = (kv.Value.Total, kv.Value.Inactive.Count);
        return result;
    }

    public static bool IsRegistered(GameObject prefab)
    {
        return Pools.ContainsKey(prefab);
    }

    public bool PresetsContains(GameObject prefab)
    {
        foreach (var p in presets)
        {
            if (p.Prefab == prefab)
                return true;
        }

        return false;
    }

    private static readonly Dictionary<GameObject, Transform> PoolRoots = new();

    private static Transform GetOrCreatePoolRoot(GameObject prefab)
    {
        if (PoolRoots.TryGetValue(prefab, out var root) && root != null)
            return root;

        var manager = Instance.transform;

        var rootName = $"{prefab.name}_Pool";
        var existing = manager.Find(rootName);
        if (existing != null)
        {
            PoolRoots[prefab] = existing;
            return existing;
        }

        var go = new GameObject(rootName);
        go.transform.SetParent(manager, false);
        PoolRoots[prefab] = go.transform;
        return go.transform;
    }

#endif
}