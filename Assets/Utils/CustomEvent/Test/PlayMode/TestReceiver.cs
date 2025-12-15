using UnityEngine;

class TestReceiver
{
    public int callCount;
    public GameObject lastObj;

    public void OnCall(TestReceiver self)
    {
        callCount++;
    }

    public void OnCallWithObj(TestReceiver self, GameObject go)
    {
        callCount++;
        lastObj = go;
    }
}