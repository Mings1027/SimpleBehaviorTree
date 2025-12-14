using UnityEngine;

public abstract class FixedUpdateBehaviour : MonoBehaviour, IFixedUpdateObserver
{
    protected virtual void OnEnable()
    {
        UpdateManager.Register(this);
    }

    protected virtual void OnDisable()
    {
        UpdateManager.DeRegister(this);
    }

    public abstract void OnFixedUpdate();
}