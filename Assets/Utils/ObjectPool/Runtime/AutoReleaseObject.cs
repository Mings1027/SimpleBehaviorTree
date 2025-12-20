using UnityEngine;

public sealed class AutoReleaseObject : MonoBehaviour
{
    internal float Delay { get; private set; }
    internal float Timer { get; private set; }
    internal bool Active { get; private set; }

    internal void StartTimer(float delay)
    {
        Delay = delay;
        Timer = 0f;
        Active = true;
    }

    internal void StopTimer()
    {
        Active = false;
    }

    internal void Tick(float dt)
    {
        if (!Active)
            return;

        Timer += dt;
    }

    internal bool IsExpired => Active && Timer >= Delay;
}