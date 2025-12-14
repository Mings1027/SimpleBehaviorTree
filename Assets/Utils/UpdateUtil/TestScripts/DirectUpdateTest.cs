using UnityEngine;

public class DirectUpdateTest : MonoBehaviour
{
    private int _value;

    private void Update()
    {
        _value = UpdateTestWork.DoWork(_value);
    }
}