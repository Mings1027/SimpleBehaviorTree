using System;
using CustomEvent;
using UnityEngine;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour, IFixedUpdateObserver, IUpdateObserver
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float angularSpeed = 90f;
    [SerializeField] private float min, max;
    private Vector3 _angularVelocity;

    public PooledEvent OnHit;

    public Player Owner { get; private set; }
    public Skill Skill { get; private set; }

    private void OnEnable()
    {
        OnHit = PooledEvent.Create();
        UpdateManager.Register(this);
    }

    private void Start()
    {
        _angularVelocity = Vector3.up * angularSpeed;
    }

    private void OnDisable()
    {
        OnHit.Dispose();
        UpdateManager.DeRegister(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnHit.Invoke(other.gameObject);
    }

    public void OnUpdate()
    {
        transform.position += Vector3.forward * (speed * Time.deltaTime);
    }

    public void OnFixedUpdate()
    {
        transform.rotation *= Quaternion.Euler(_angularVelocity * Time.fixedDeltaTime);
    }

    public void Init(Player owner, Skill skill)
    {
        Owner = owner;
        Skill = skill;
    }
}