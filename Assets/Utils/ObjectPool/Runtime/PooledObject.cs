using UnityEngine;

[DisallowMultipleComponent]
public sealed class PooledObject : MonoBehaviour
{
    private GameObject _prefab;

    public bool IsInPool { get; private set; }

    internal void Init(GameObject prefab)
    {
        _prefab = prefab;
        IsInPool = true;
    }

    internal void MarkInUse()
    {
        IsInPool = false;
    }

    internal void MarkReleased()
    {
        IsInPool = true;
    }

    public void ReturnToPool()
    {
        if (IsInPool) return;
        
        if (!ObjectPoolManager.HasInstance)
            return;

        ObjectPoolManager.Release(_prefab, gameObject);
    }
}