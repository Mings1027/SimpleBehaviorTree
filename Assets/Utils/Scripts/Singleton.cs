using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

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

                    var singleton = _instance as Singleton<T>;
                    if (singleton != null && singleton.useDontDestroyOnLoad)
                        DontDestroyOnLoad(go);
                }
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

        if (useDontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }
}