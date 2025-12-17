using UnityEngine;

public sealed class AutoReleaseObject : MonoBehaviour
{
    public float Delay { get; private set; }
    public float Timer { get; private set; }
    public bool Active { get; private set; }

    public void StartTimer(float delay)
    {
        Delay = delay;
        Timer = 0f;
        Active = true;
    }

    public void StopTimer()
    {
        Active = false;
    }

    public void Tick(float dt)
    {
        if (!Active)
            return;

        Timer += dt;
    }

    public bool IsExpired => Active && Timer >= Delay;
}