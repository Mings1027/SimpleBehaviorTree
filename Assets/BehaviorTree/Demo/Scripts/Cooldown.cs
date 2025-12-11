using System;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class Cooldown
    {
        public Cooldown() { }

        public Cooldown(float cooldownTime)
        {
            this.cooldownTime = cooldownTime;
        }

        private float _nextFireTime;

        [SerializeField] private float cooldownTime;

        public bool IsCoolingDown => Time.time < _nextFireTime;
        public void StartCooldown() => _nextFireTime = Time.time + cooldownTime;

        public void Reset() => _nextFireTime = 0;
    }
}