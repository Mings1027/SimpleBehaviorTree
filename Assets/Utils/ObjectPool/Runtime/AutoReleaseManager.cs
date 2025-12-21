using System.Collections.Generic;
using UnityEngine;

public class AutoReleaseManager : MonoBehaviour
{
    private List<AutoReleaseObject> _actives = new();

    internal void Register(AutoReleaseObject obj)
    {
        if (!_actives.Contains(obj))
            _actives.Add(obj);
    }

    internal void Unregister(AutoReleaseObject obj)
    {
        _actives.Remove(obj);
    }

    private void Update()
    {
        for (int i = _actives.Count - 1; i >= 0; i--)
        {
            var obj = _actives[i];

            obj.Tick(Time.deltaTime);

            if (!obj.IsExpired)
                continue;

            obj.StopTimer();
            _actives.RemoveAt(i);

            if (obj.TryGetComponent(out PooledObject pooled))
            {
                pooled.ReturnToPool();
            }
        }
    }
}