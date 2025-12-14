using UnityEngine;

[RequireComponent(typeof(PooledObject)), DisallowMultipleComponent]
public sealed class AutoReleaseToPool : MonoBehaviour
{
    private float _delay;
    private PooledObject _pooled;

    internal float ReleaseAt { get; private set; } = -1f;

    internal void Init(PooledObject pooled, float delay)
    {
        _pooled = pooled;
        _delay = delay;
    }

    internal void StartTimer()
    {
        ReleaseAt = Time.time + _delay;
    }

    internal void CancelTimer()
    {
        ReleaseAt = -1f;
    }

    internal void Release()
    {
        _pooled.Release();
    }
}