using System;
using UnityEngine;
using Utils;

public class AutoReleaseToPool : UpdateBehaviour
{
    private GameObject _prefab;
    private float _delay;
    private float _releaseAt;
    private bool _inUse;

    public void Init(GameObject prefab, float delay)
    {
        _prefab = prefab;
        _delay = delay;
    }

    public void OnGet()
    {
        _inUse = true;

        if (_delay > 0f)
        {
            _releaseAt = Time.time + _delay;
        }
    }

    public void OnRelease()
    {
        _inUse = false;
    }

    public override void OnUpdate()
    {
        if (!_inUse || _delay <= 0f)
            return;

        if (Time.time >= _releaseAt)
        {
            ObjectPoolManager.Release(_prefab, gameObject);
        }
    }
}