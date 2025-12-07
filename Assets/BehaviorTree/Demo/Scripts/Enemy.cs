using System;
using BehaviorTree;
using BehaviorTree.Demo.Scripts.EnemyAI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private BehaviorTreeController _treeController;

    [SerializeField] private AIContext blackboard;
    [SerializeField] private Transform waitTransform;

    private void Awake()
    {
        _treeController = GetComponent<BehaviorTreeController>();
    }

    private void Start()
    {
        _treeController.CreateTree(CreateTree());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, blackboard.detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blackboard.attackRange);
    }

    private Node CreateTree()
    {
        blackboard.self = transform;
        blackboard.waitPosition = waitTransform;

        var root = new SelectorNode();

        var combatSequence = new SequenceNode();
        var detectParallel = new ParallelNode(ParallelPolicy.And);
        var attackSelector  = new SelectorNode();
        var attackSequence = new SequenceNode();
        
        root.AddChild(combatSequence);
        root.AddChild(new MoveToWaitPositionNode(blackboard));

        combatSequence.AddChild(detectParallel);
        combatSequence.AddChild(new HasTargetNode(blackboard));
        combatSequence.AddChild(attackSelector);

        detectParallel.AddChild(new CooldownWaitNode(blackboard));
        detectParallel.AddChild(new FindTargetNode(blackboard));

        attackSelector.AddChild(attackSequence);
        attackSelector.AddChild(new MoveToTargetNode(blackboard));

        attackSequence.AddChild(new IsInAttackRangeNode(blackboard));
        attackSequence.AddChild(new CanAttackNode(blackboard));
        attackSequence.AddChild(new AttackNode(blackboard));

        return root;
    }
}