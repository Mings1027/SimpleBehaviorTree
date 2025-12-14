using UnityEngine;

public abstract class UpdateBehaviour : MonoBehaviour, IUpdateObserver
{
    protected void OnEnable()
    {
        UpdateManager.Register(this);
    }

    protected virtual void OnDisable()
    {
        UpdateManager.DeRegister(this);
    }

    public abstract void OnUpdate();
}