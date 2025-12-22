using System;
using UnityEngine;
using CustomEvent;

public class EventHostA : MonoBehaviour
{
    // 1) A 내부 Create+Bind+Invoke
    internal PooledEvent eventA1;

    // 2) A 생성, B가 Bind, A가 Invoke
    internal PooledEvent eventA2;

    // 3) A 생성, B가 Bind, A가 InvokeEvent
    internal PooledEvent eventA3;

    // 4) A 내부 Create + Bind + InvokeEvent
    internal PooledEvent eventA4;

    // 5) Space로 실행되는 OneShot
    internal PooledEvent eventA5;

    public PooledEvent OnHit;

    private void OnEnable()
    {
        eventA1 = PooledEvent.Create().Bind(this, t => t.OnEventA1());
        eventA2 =  PooledEvent.Create();
        eventA3 =  PooledEvent.Create();
        eventA4 = PooledEvent.Create().Bind(this, (t, obj) => t.OnEventA4(obj));
        eventA5 = PooledEvent.Create().Bind(this, t => t.OnEventA5());
        OnHit =  PooledEvent.Create();
    }

    private void OnDisable()
    {
        eventA1.Dispose();
        eventA2.Dispose();
        eventA3.Dispose();
        eventA4.Dispose();
        eventA5.Dispose();
        OnHit.Dispose();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            eventA1.Invoke();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            eventA2.Invoke();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            eventA3.Invoke(gameObject);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            eventA4.Invoke(gameObject);

        if (Input.GetKeyDown(KeyCode.Space))
            eventA5.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHit.Invoke(other.gameObject);
    }

    private void OnEventA1()
    {
        Debug.Log("[EventA1] A 내부 Create+Bind+Invoke 성공");
    }

    private void OnEventA4(GameObject go)
    {
        Debug.Log($"[EventA4] A 내부 GameObject 이벤트 호출됨, 전달 GO = {go.name}");
    }

    private void OnEventA5()
    {
        Debug.Log("[EventA5 OneShot] 단 한 번 실행됨");
    }
}