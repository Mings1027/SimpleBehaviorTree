using UnityEngine;

public abstract class LateUpdateBehaviour : MonoBehaviour, ILateUpdateObserver
{
    protected virtual void OnEnable()
    {
        UpdateManager.Register(this);
    }

    protected virtual void OnDisable()
    {
        UpdateManager.DeRegister(this);
    }

    public abstract void OnLateUpdate();
}