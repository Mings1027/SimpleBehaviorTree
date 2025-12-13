using System;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class TweenTest : MonoBehaviour
    {
        [SerializeField] private Cooldown cooldown;
        private Tween _tween;

        private void Update()
        {
            if (cooldown.IsCoolingDown) return;

            cooldown.StartCooldown();
            StartRandomMove();
        }

        private void FixedUpdate()
        {
            
        }

        private void LateUpdate()
        {
            
        }

        private void StartRandomMove()
        {
            var randomPos = new Vector3(Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10));
            _tween.Stop();
            _tween = Tween.Position(transform, randomPos, cooldown.Duration);
        }
    }
}