using BehaviorTree;
using UnityEngine;

public class BehaviorTreePhysicsMover : MonoBehaviour
{
    private Rigidbody rigid;
    private Blackboard blackboard;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.useGravity = false;

        var controller = GetComponent<BehaviorTreeController>();
        blackboard = controller.Blackboard;
    }

    private void FixedUpdate()
    {
        rigid.MovePosition(rigid.position + blackboard.MoveDirection * (blackboard.MoveSpeed * Time.fixedDeltaTime));
    }
}