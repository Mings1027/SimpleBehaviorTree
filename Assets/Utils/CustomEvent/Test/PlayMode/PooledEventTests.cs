using NUnit.Framework;
using UnityEngine;
using CustomEvent;

public class PooledEventTests
{
    [Test]
    public void Invoke_WithoutCreate_DoesNothing()
    {
        PooledEvent evt = default;

        evt.Invoke();
        evt.InvokeEvent(null);

        Assert.Pass();
    }

    [Test]
    public void CreateBindInvoke_Works()
    {
        var receiver = new TestReceiver();

        var evt = PooledEvent.Create()
            .Bind(receiver, r => r.OnCall(r));

        evt.Invoke();

        Assert.AreEqual(1, receiver.callCount);

        evt.Dispose();
    }

    [Test]
    public void InvokeEvent_WithGameObject_Works()
    {
        var receiver = new TestReceiver();
        var go = new GameObject("Test");

        var evt = PooledEvent.Create()
            .Bind(receiver, (r, obj) => r.OnCallWithObj(r, obj));

        evt.InvokeEvent(go);

        Assert.AreEqual(1, receiver.callCount);
        Assert.AreEqual(go, receiver.lastObj);

        evt.Dispose();
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Invoke_AfterDispose_DoesNothing()
    {
        var receiver = new TestReceiver();

        var evt = PooledEvent.Create()
            .Bind(receiver, r => r.OnCall(r));

        evt.Dispose();

        evt.Invoke();
        evt.InvokeEvent(null);

        Assert.AreEqual(0, receiver.callCount);
    }

    [Test]
    public void Reuse_FromPool_DoesNotLeakCallback()
    {
        var receiverA = new TestReceiver();
        var receiverB = new TestReceiver();

        var evt = PooledEvent.Create()
            .Bind(receiverA, r => r.OnCall(r));

        evt.Invoke();
        evt.Dispose();

        evt = PooledEvent.Create()
            .Bind(receiverB, r => r.OnCall(r));

        evt.Invoke();

        Assert.AreEqual(1, receiverA.callCount);
        Assert.AreEqual(1, receiverB.callCount);

        evt.Dispose();
    }
}