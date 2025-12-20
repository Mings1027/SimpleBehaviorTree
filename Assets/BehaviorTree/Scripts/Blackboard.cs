using System;
using UnityEngine;

namespace BehaviorTree
{
    [Serializable]
    public abstract class Blackboard
    {
        public Vector3 MoveDirection;
        public float MoveSpeed;
    }
}