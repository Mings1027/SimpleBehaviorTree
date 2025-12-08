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

        var combatSequence = CreateCombatTree();
        var waitParallel = CreateWaitTree();
        
        root.AddChild(combatSequence);
        root.AddChild(waitParallel);
        
        return root;
    }

    private Node CreateCombatTree()
    {
        var combatSequence = new SequenceNode();
        combatSequence.AddChild(new HasTargetNode(blackboard));
        combatSequence.AddChild(new TargetValidNode(blackboard));

        var combatSelector = new SelectorNode();
        combatSequence.AddChild(combatSelector);

        var attackBranch = new SequenceNode();
        attackBranch.AddChild(new CanAttackNode(blackboard));

        var attackParallel = new ParallelNode(ParallelPolicy.And);
        attackBranch.AddChild(attackParallel);

        attackParallel.AddChild(new FindTargetNode(blackboard));

        var attackOrChaseSelector = new SelectorNode();
        attackParallel.AddChild(attackOrChaseSelector);

        var attackSequence = new SequenceNode();
        attackSequence.AddChild(new IsInAttackRangeNode(blackboard));
        attackSequence.AddChild(new AttackNode(blackboard));

        attackOrChaseSelector.AddChild(attackSequence);
        attackOrChaseSelector.AddChild(new MoveToTargetNode(blackboard));

        combatSelector.AddChild(attackBranch);

        var cooldownParallel = new ParallelNode(ParallelPolicy.And);
        cooldownParallel.AddChild(new CooldownWaitNode(blackboard));
        cooldownParallel.AddChild(new FindTargetNode(blackboard));

        combatSelector.AddChild(cooldownParallel);

        return combatSequence;
    }

    private Node CreateWaitTree()
    {
        var waitParallel = new ParallelNode(ParallelPolicy.And);
        waitParallel.AddChild(new MoveToWaitPositionNode(blackboard));
        waitParallel.AddChild(new FindTargetNode(blackboard));
        return waitParallel;
    }
}