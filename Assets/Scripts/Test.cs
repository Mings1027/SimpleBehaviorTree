using System;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"Enter - {other.gameObject.name}" );
    }

    private void OnCollisionStay(Collision other)
    {
        Debug.Log($"Stay - {other.gameObject.name}" );
    }

    private void OnCollisionExit(Collision other)
    {
        Debug.Log($"Exit - {other.gameObject.name}" );  
    }
}
