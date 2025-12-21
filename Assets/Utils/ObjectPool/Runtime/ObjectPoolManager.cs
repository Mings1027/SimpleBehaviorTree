using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utils;
using Debug = UnityEngine.Debug;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private static readonly Dictionary<GameObject, Stack<PooledObject>> poolTable = new();
    private static readonly Dictionary<GameObject, PoolEntry> entryTable = new();

    private static AutoReleaseManager autoReleaseManager;

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
            Register(entry.Prefab, entry.PreloadCount, entry.UseAutoRelease, entry.AutoReleaseDelay);

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
        if (autoReleaseManager != null) return;

        var go = new GameObject("[AutoReleaseManager]");
        go.transform.SetParent(Instance.transform, false);
        autoReleaseManager = go.AddComponent<AutoReleaseManager>();
    }

    /// <summary>
    /// 새로운 오브젝트 풀을 등록하고 미리 인스턴스를 생성합니다.
    /// 이미 등록된 키(프리팹)일 경우 경고 로그를 출력하고 무시합니다.
    /// </summary>
    /// <param name="prefab">풀의 키(Key)로 사용될 원본 프리팹입니다.</param>
    /// <param name="preloadCount">초기에 미리 생성해둘 인스턴스의 개수입니다.</param>
    /// <param name="useAutoRelease">True일 경우, 가져간(Get) 오브젝트가 일정 시간 후 자동으로 반환됩니다.</param>
    /// <param name="autoReleaseDelay">자동 반환될 때까지의 대기 시간(초)입니다.</param>
    public static void Register(GameObject prefab, int preloadCount = 0, bool useAutoRelease = false,
        float autoReleaseDelay = 0f)
    {
        if (prefab == null)
            return;

        _ = Instance;

        if (poolTable.ContainsKey(prefab))
        {
            Debug.LogWarning($"[ObjectPool] Duplicate Registration: '{prefab.name}'");
            return;
        }

        var entry = new PoolEntry
        {
            Prefab = prefab,
            PreloadCount = preloadCount,
            UseAutoRelease = useAutoRelease,
            AutoReleaseDelay = autoReleaseDelay
        };
        entryTable[prefab] = entry;

        if (useAutoRelease && autoReleaseDelay > 0f)
        {
            CreateAutoReleaseManager();
        }

        var stack = new Stack<PooledObject>();
        poolTable[prefab] = stack;

        for (int i = 0; i < preloadCount; i++)
        {
            var pooled = CreateInstance(prefab);
            pooled.gameObject.SetActive(false);
            stack.Push(pooled);
        }
    }

    #region Get

    /// <summary>
    /// 풀에서 오브젝트를 가져와 지정된 Transform의 위치와 회전으로 설정합니다.
    /// </summary>
    /// <param name="poolKey">가져올 풀의 키(원본 프리팹)입니다.</param>
    /// <param name="t">위치와 회전을 참조할 Transform입니다.</param>
    /// <param name="setActive">가져온 직후 오브젝트를 활성화할지 여부입니다.</param>
    /// <returns>풀에서 꺼낸(혹은 생성된) 게임 오브젝트 인스턴스입니다.</returns>
    public static GameObject Get(GameObject poolKey, Transform t, bool setActive = true) =>
        Get(poolKey, t.position, t.rotation, setActive);

    /// <summary>
    /// 풀에서 오브젝트를 가져와 지정된 위치에 배치합니다 (회전은 identity).
    /// </summary>
    /// <param name="poolKey">가져올 풀의 키(원본 프리팹)입니다.</param>
    /// <param name="position">오브젝트가 배치될 월드 좌표입니다.</param>
    /// <param name="setActive">가져온 직후 오브젝트를 활성화할지 여부입니다.</param>
    /// <returns>풀에서 꺼낸(혹은 생성된) 게임 오브젝트 인스턴스입니다.</returns>
    public static GameObject Get(GameObject poolKey, Vector3 position, bool setActive = true) =>
        Get(poolKey, position, Quaternion.identity, setActive);

    /// <summary>
    /// 풀에서 오브젝트를 가져와 지정된 위치와 회전으로 설정합니다.
    /// </summary>
    /// <param name="poolKey">가져올 풀의 키(원본 프리팹)입니다.</param>
    /// <param name="position">오브젝트가 배치될 월드 좌표입니다.</param>
    /// <param name="rotation">오브젝트의 회전값입니다.</param>
    /// <param name="setActive">가져온 직후 오브젝트를 활성화할지 여부입니다.</param>
    /// <returns>풀에서 꺼낸(혹은 생성된) 게임 오브젝트 인스턴스입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GameObject Get(GameObject poolKey, Vector3 position, Quaternion rotation, bool setActive = true)
    {
        var instance = Get(poolKey, setActive);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    /// <summary>
    /// 풀에서 오브젝트를 가져옵니다. 풀이 비어있다면 새로 생성합니다.
    /// 등록되지 않은 프리팹을 요청하면 자동으로 풀을 등록합니다.
    /// </summary>
    /// <param name="poolKey">가져올 풀의 키(원본 프리팹)입니다.</param>
    /// <param name="setActive">가져온 직후 오브젝트를 활성화할지 여부입니다.</param>
    /// <returns>풀에서 꺼낸(혹은 생성된) 게임 오브젝트 인스턴스입니다.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GameObject Get(GameObject poolKey, bool setActive = true)
    {
        if (poolKey == null) return null;

        if (!poolTable.TryGetValue(poolKey, out var stack))
        {
            if (Instance.poolRegistry != null &&
                Instance.poolRegistry.TryGetEntry(poolKey, out var entry))
            {
                // 레지스트리 설정대로 등록
                Register(entry.Prefab, entry.PreloadCount, entry.UseAutoRelease, entry.AutoReleaseDelay);
            }
            else
            {
                // 레지스트리에도 없다면 기본값으로 등록 (경고 출력)
#if UNITY_EDITOR
                Debug.LogWarning($"[ObjectPool] Auto-registered prefab (No settings found): {poolKey.name}");
#endif
                Register(poolKey);
            }

            stack = poolTable[poolKey];
        }

        PooledObject pooled = null;
        while (stack.Count > 0)
        {
            var item = stack.Pop();
            if (!ReferenceEquals(item, null) && !item.IsDestroyed)
            {
                pooled = item;
                break;
            }
        }

        if (ReferenceEquals(pooled, null)) pooled = CreateInstance(poolKey);
        pooled.MarkInUse();

        // AutoRelease 처리
        if (entryTable.TryGetValue(poolKey, out var poolEntry) &&
            poolEntry.UseAutoRelease && poolEntry.AutoReleaseDelay > 0f)
        {
            var auto = pooled.TryGetOrAddComponent<AutoReleaseObject>();
            auto.StartTimer(poolEntry.AutoReleaseDelay);
            
            autoReleaseManager.Register(auto);
        }

        pooled.gameObject.SetActive(setActive);
        return pooled.gameObject;
    }

    #endregion

    #region Release

    /// <summary>
    /// 사용이 끝난 인스턴스를 풀에 반환합니다.
    /// </summary>
    /// <param name="poolKey">인스턴스가 속한 풀의 키(원본 프리팹)입니다.</param>
    /// <param name="instance">반환할 게임 오브젝트 인스턴스입니다.</param>
    public static void Release(GameObject poolKey, GameObject instance)
    {
        if (!HasInstance || poolKey == null || instance == null) return;

        if (instance.TryGetComponent(out PooledObject pooled))
        {
            Release(poolKey, pooled);
        }
    }

    public static void Release(GameObject poolKey, PooledObject pooled)
    {
        if (!HasInstance || poolKey == null || pooled == null) return;

        if (!poolTable.TryGetValue(poolKey, out var stack)) return;

        // AutoRelease 처리
        if (pooled.TryGetComponent(out AutoReleaseObject auto))
        {
            auto.StopTimer();
            autoReleaseManager?.Unregister(auto);
        }

        if (pooled.IsInPool) return;
        pooled.MarkReleased();

        pooled.gameObject.SetActive(false);

#if UNITY_EDITOR
        SetEditorPoolParent(poolKey, pooled.transform);
#endif

        stack.Push(pooled); // [변경] PooledObject 자체를 Push
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PooledObject CreateInstance(GameObject prefab)
    {
        var instance = Instantiate(prefab);
#if UNITY_EDITOR
        instance.name = prefab.name;
#endif

        var pooled = instance.TryGetOrAddComponent<PooledObject>();
        pooled.Init(prefab);

#if UNITY_EDITOR
        SetEditorPoolParent(prefab, instance.transform);
#endif

        return pooled;
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