using System;
using UnityEngine;

namespace BehaviorTree.Demo.Scripts.EnemyAI
{
    [Serializable]
    public class AIContext : Blackboard
    {
        public Transform self;
        public Transform target;
        public Transform waitPosition;
        
        public float detectionRange = 10f;
        public float attackRange = 2f;
        public float moveSpeed = 3f;

        // OverlapSphereNonAlloc 에서 사용할 타겟 레이어
        public LayerMask targetMask;

        // OverlapSphereNonAlloc 용 버퍼 (30개 고정)
        public Collider[] targetBuffer = new Collider[30];
        
        public Cooldown attackCooldown = new();
    }
}