using System;
using UnityEngine;

public class Projectile : MonoBehaviour, IFixedUpdateObserver, IUpdateObserver
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float angularSpeed = 90f;

    private Vector3 _angularVelocity;

    private void OnEnable()
    {
        UpdateManager.Register(this);
    }

    private void Start()
    {
        _angularVelocity = Vector3.up * angularSpeed;
    }

    private void OnDisable()
    {
        UpdateManager.DeRegister(this);
    }

    public void OnUpdate()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    public void OnFixedUpdate()
    {
        transform.rotation *= Quaternion.Euler(_angularVelocity * Time.fixedDeltaTime);
    }
}