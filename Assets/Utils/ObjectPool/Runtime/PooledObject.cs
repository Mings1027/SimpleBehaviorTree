using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PooledObject : MonoBehaviour
{
    private GameObject _poolKey;

    internal bool IsDestroyed { get; private set; }
    
    internal bool IsInPool { get; private set; }

    private void OnDestroy()
    {
        IsDestroyed = true;
    }

    internal void Init(GameObject prefab)
    {
        _poolKey = prefab;
        IsInPool = true;
        IsDestroyed = false;
    }

    internal void MarkInUse()
    {
        IsInPool = false;
    }

    internal void MarkReleased()
    {
        IsInPool = true;
    }

    internal void ReturnToPool()
    {
        if (IsInPool) return;
        
        ObjectPoolManager.Release(_poolKey, this);
    }
}