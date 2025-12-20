using BehaviorTree;
using UnityEngine;

public class BehaviorTreeMover : MonoBehaviour
{
    [SerializeReference] private Blackboard blackboard;

    private void Awake()
    {
        var controller = GetComponent<BehaviorTreeController>();
        blackboard = controller.Blackboard;
    }

    private void Update()
    {
        transform.position += blackboard.MoveDirection * (blackboard.MoveSpeed * Time.deltaTime);
    }
}