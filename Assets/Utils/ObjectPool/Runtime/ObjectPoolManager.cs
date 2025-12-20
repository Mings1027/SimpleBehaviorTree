using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private class Pool
    {
        public readonly Stack<GameObject> Inactive = new();
    }

    private static readonly Dictionary<GameObject, Pool> poolTable = new();
    private static readonly Dictionary<GameObject, PoolEntry> entryTable = new();

    private static AutoReleaseManager autoReleaseManager;

    public static bool HasInstance => _instance != null;

    [SerializeField] private PoolRegistry poolRegistry;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        _instance = null;
        poolTable.Clear();
        entryTable.Clear();
#if UNITY_EDITOR
        poolRoots.Clear();
#endif
        autoReleaseManager = null;
    }

    protected override void Awake()
    {
        base.Awake();
        RegisterFromRegistry();
    }

    // -----------------------------
    // Registry
    // -----------------------------
    private void RegisterFromRegistry()
    {
        if (poolRegistry == null)
            return;

        bool needAutoRelease = false;
        foreach (var entry in poolRegistry.Entries)
        {
            if (entry.Prefab == null)
                continue;

            entryTable[entry.Prefab] = entry;
            Register(entry.Prefab, entry.PreloadCount);

            if (entry.UseAutoRelease && entry.AutoReleaseDelay > 0f)
                needAutoRelease = true;
        }

        if (needAutoRelease)
        {
            CreateAutoReleaseManager();
        }
    }

    // ----------------------------------
    // AutoReleaseManager
    // ----------------------------------
    private static void CreateAutoReleaseManager()
    {
        if (autoReleaseManager != null)
            return;

        var go = new GameObject("[AutoReleaseManager]");
        go.transform.SetParent(Instance.transform, false);
        autoReleaseManager = go.AddComponent<AutoReleaseManager>();
    }


    // -----------------------------
    // Register
    // -----------------------------
    public static void Register(GameObject prefab, int preloadCount = 0)
    {
        if (prefab == null)
            return;

        _ = Instance;

        if (poolTable.ContainsKey(prefab))
            return;

        var pool = new Pool();
        poolTable[prefab] = pool;

        for (int i = 0; i < preloadCount; i++)
        {
            var instance = CreateInstance(prefab);
            instance.SetActive(false);
            pool.Inactive.Push(instance);
        }
    }

    #region Get

    public static GameObject Get(GameObject prefab, Vector3 position, bool setActive = true)
    {
        return Get(prefab, position, Quaternion.identity, setActive);
    }

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, bool setActive = true)
    {
        var instance = Get(prefab, setActive);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public static GameObject Get(GameObject prefab, bool setActive = true)
    {
        if (prefab == null)
            return null;

        if (!poolTable.TryGetValue(prefab, out var pool))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[ObjectPool] Auto-registered prefab: {prefab.name}");
#endif
            Register(prefab);
            pool = poolTable[prefab];
        }

        var instance = pool.Inactive.Count > 0
            ? pool.Inactive.Pop()
            : CreateInstance(prefab);

        if (instance.TryGetComponent(out PooledObject pooled))
            pooled.MarkInUse();

        // AutoRelease 처리
        if (entryTable.TryGetValue(prefab, out var entry) &&
            entry.UseAutoRelease && entry.AutoReleaseDelay > 0f)
        {
            var auto = instance.TryGetOrAddComponent<AutoReleaseObject>();
            auto.StartTimer(entry.AutoReleaseDelay);
            autoReleaseManager?.Register(auto);
        }

        instance.SetActive(setActive);
        return instance;
    }

    #endregion

    #region Release

    public static void Release(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
            return;

        if (!poolTable.TryGetValue(prefab, out var pool))
            return;

        if (instance.TryGetComponent(out AutoReleaseObject auto))
        {
            auto.StopTimer();
            autoReleaseManager?.Unregister(auto);
        }

        if (instance.TryGetComponent(out PooledObject pooled))
        {
            if (pooled.IsInPool)
                return;

            pooled.MarkReleased();
        }

        instance.SetActive(false);

#if UNITY_EDITOR
        SetEditorPoolParent(prefab, instance.transform);
#endif

        pool.Inactive.Push(instance);
    }

    #endregion

    private static GameObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        instance.name = prefab.name;

        var pooled = instance.TryGetOrAddComponent<PooledObject>();
        pooled.Init(prefab);

#if UNITY_EDITOR
        SetEditorPoolParent(prefab, instance.transform);
#endif

        return instance;
    }

    [Conditional("UNITY_EDITOR")]
    private static void SetEditorPoolParent(GameObject prefab, Transform instance)
    {
        var root = GetOrCreatePoolRoot(prefab);
        instance.SetParent(root, false);
    }

#if UNITY_EDITOR
    private static readonly Dictionary<GameObject, Transform> poolRoots = new();

    private static Transform GetOrCreatePoolRoot(GameObject prefab)
    {
        if (poolRoots.TryGetValue(prefab, out var root) && root != null)
            return root;

        var rootName = $"[{prefab.name}]_Pool";
        var go = new GameObject(rootName);
        go.transform.SetParent(Instance.transform, false);

        poolRoots[prefab] = go.transform;
        return go.transform;
    }
#endif
}