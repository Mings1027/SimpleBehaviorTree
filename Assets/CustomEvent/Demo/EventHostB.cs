using UnityEngine;

public class EventHostB : MonoBehaviour
{
    private EventHostA _hostA;

    private void OnEnable()
    {
        _hostA = FindAnyObjectByType<EventHostA>(FindObjectsInactive.Include);

        if (_hostA == null)
        {
            Debug.LogError("EventHostA를 찾을 수 없음.");
        }

        _hostA.eventA2.Bind(this, t => t.OnEventA2_FromB());
        _hostA.eventA3.Bind(this, (t, obj) => t.OnEventA3_FromB(obj));
        _hostA.OnHit.Bind(this, (t, obj) => t.Hit(obj));
    }

    private void OnEventA2_FromB()
    {
        Debug.Log("[EventA2] B에서 Bind됨 → A에서 Invoke");
    }

    private void OnEventA3_FromB(GameObject go)
    {
        Debug.Log($"[EventA3] B에서 Bind됨 → A의 InvokeEvent 호출됨, 전달 GO = {go.name}");
    }

    private void Hit(GameObject obj)
    {
        Debug.Log(obj);
    }
}