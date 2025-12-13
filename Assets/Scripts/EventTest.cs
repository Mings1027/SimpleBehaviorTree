using System;
using CustomEvent;
using PrimeTween;
using UnityEngine;

namespace DefaultNamespace
{
    public class EventTest: MonoBehaviour
    {
        public PooledEvent  OnHit;

        private void Awake()
        {
            OnHit = PooledEvent.Create();
        }

        private void OnTriggerEnter(Collider other)
        {
            OnHit.InvokeEvent(other.gameObject);
        }
    }
}