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
        blackboard.attackCooldown.StartCooldown();

        var root = new ParallelNode(
            ParallelPolicy.And,

            // 항상 타겟 갱신
            new FindTargetNode(blackboard),
            new SelectorNode(
                // 1) 쿨타임 중 → 무조건 제자리 유지
                new CooldownWaitNode(blackboard),

                // 2) 타겟이 있으면 전투 시퀀스
                new SequenceNode(
                    new HasTargetNode(blackboard),
                    new SelectorNode(
                        // 공격 시도
                        new SequenceNode(
                            new IsInAttackRangeNode(blackboard),
                            new CanAttackNode(blackboard),
                            new AttackNode(blackboard)
                        ),

                        // 공격 불가능 → 이동
                        new MoveToTargetNode(blackboard)
                    )
                ),

                // 3) 타겟이 없으면 대기 위치로 이동
                new MoveToWaitPositionNode(blackboard)
            )
        );

        return root;
    }

    private Node CreateTreeTest()
    {
        blackboard.self = transform;
        blackboard.waitPosition = waitTransform;
        blackboard.attackCooldown.StartCooldown();

        var root = new ParallelNode(ParallelPolicy.And);
        root.AddChild(new FindTargetNode(blackboard));

        var behaviorSelector = new SelectorNode();
        root.AddChild(behaviorSelector);

        behaviorSelector.AddChild(new CooldownWaitNode(blackboard));

        var fightSequence = new SequenceNode();
        behaviorSelector.AddChild(fightSequence);

        fightSequence.AddChild(new HasTargetNode(blackboard));

        var attackOrMoveSelector = new SelectorNode();
        fightSequence.AddChild(attackOrMoveSelector);

        var attackSequence = new SequenceNode();
        attackSequence.AddChild(new IsInAttackRangeNode(blackboard));
        attackSequence.AddChild(new CanAttackNode(blackboard));
        attackSequence.AddChild(new AttackNode(blackboard));
        attackOrMoveSelector.AddChild(attackSequence);

        attackOrMoveSelector.AddChild(new MoveToTargetNode(blackboard));

        behaviorSelector.AddChild(new MoveToWaitPositionNode(blackboard));

        return root;
    }
}