using System.Collections.Generic;
using UnityEngine;

public class AutoReleaseManager : MonoBehaviour
{
    private readonly List<AutoReleaseObject> actives = new();

    public void Register(AutoReleaseObject obj)
    {
        if (!actives.Contains(obj))
            actives.Add(obj);
    }

    public void Unregister(AutoReleaseObject obj)
    {
        actives.Remove(obj);
    }

    private void Update()
    {
        for (int i = actives.Count - 1; i >= 0; i--)
        {
            var obj = actives[i];

            obj.Tick(Time.deltaTime);

            if (!obj.IsExpired)
                continue;

            obj.StopTimer();
            actives.RemoveAt(i);

            if (obj.TryGetComponent(out PooledObject pooled))
            {
                pooled.ReturnToPool();
            }
        }
    }
}