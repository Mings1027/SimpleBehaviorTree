using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    private static Transform _sceneRoot;
    private static Transform _dontDestroyRoot;

    private const string SceneRootName = "[Singletons]";
    private const string DontDestroyRootName = "[DontDestroy Singletons]";

    /// <summary>
    /// 싱글톤 인스턴스가 현재 존재하는지 확인합니다.
    /// 씬 종료 시점이나 파괴 과정에서 불필요한 인스턴스 생성을 방지하기 위해 사용됩니다.
    /// </summary>
    public static bool HasInstance => _instance != null;
    
    protected static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    var go = new GameObject($"[{typeof(T).Name}]");
                    _instance = go.AddComponent<T>();
                }

#if UNITY_EDITOR
                var s = _instance as Singleton<T>;
                AssignParent(_instance.transform, s != null && s.useDontDestroyOnLoad);
#endif
            }

            return _instance;
        }
    }

    [SerializeField] private bool useDontDestroyOnLoad;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
#if UNITY_EDITOR
        AssignParent(transform, useDontDestroyOnLoad);
#endif
        if (useDontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    // ----------------------------
    // Parent Management
    // ----------------------------
#if UNITY_EDITOR
    private static void AssignParent(Transform child, bool useDontDestroy)
    {
        var root = useDontDestroy
            ? GetOrCreateDontDestroyRoot()
            : GetOrCreateSceneRoot();

        if (child.parent != root)
            child.SetParent(root, false);
    }

    private static Transform GetOrCreateSceneRoot()
    {
        if (_sceneRoot != null)
            return _sceneRoot;

        var obj = GameObject.Find(SceneRootName);
        if (obj == null)
            obj = new GameObject(SceneRootName);

        _sceneRoot = obj.transform;
        return _sceneRoot;
    }

    private static Transform GetOrCreateDontDestroyRoot()
    {
        if (_dontDestroyRoot != null)
            return _dontDestroyRoot;

        var obj = GameObject.Find(DontDestroyRootName);
        if (obj == null)
        {
            obj = new GameObject(DontDestroyRootName);
            DontDestroyOnLoad(obj);
        }

        _dontDestroyRoot = obj.transform;
        return _dontDestroyRoot;
    }
#endif
}