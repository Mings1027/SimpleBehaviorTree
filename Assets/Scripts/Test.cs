using System;
using PrimeEvent;
using UnityEngine;
using Utilities;

public class Test : MonoBehaviour
{
    private CustomEvent customEvent;
    private Cooldown cooldown;

    [SerializeField] private float cooldownTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cooldown = new Cooldown(cooldownTime);
        customEvent = CustomEvent.Create();
        customEvent.OnSet(this, t => t.DoSomething());
    }

    private void Update()
    {
        if (cooldown.IsCoolingDown) return;

        customEvent.Invoke();
        cooldown.StartCooldown();
    }

    private void DoSomething()
    {
        Debug.Log("do something!");
    }
}