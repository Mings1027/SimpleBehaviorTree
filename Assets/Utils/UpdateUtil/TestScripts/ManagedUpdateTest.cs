using UnityEngine;

public class ManagedUpdateTest : UpdateBehaviour
{
    private int _value;

    public override void OnUpdate()
    {
        _value = UpdateTestWork.DoWork(_value);
    }
}