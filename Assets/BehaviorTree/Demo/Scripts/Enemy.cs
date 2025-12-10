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

        // 1) 공격 쿨타임 상태: 제자리에서 타겟 검색만
        var cooldownTree = CreateCooldownTree();

        // 2) 전투 상태: 타겟 있으면 추적/공격
        var combatTree = CreateCombatTree();

        // 3) 대기 상태: 대기 위치로 이동하면서 타겟 검색
        var waitTree = CreateWaitTree();

        root.AddChild(cooldownTree);
        root.AddChild(combatTree);
        root.AddChild(waitTree);

        return root;
    }

    /// <summary>
    /// 공격 직후 ~ 쿨타임 동안:
    /// - 이동/대기 이동 X
    /// - 그 자리에서 타겟 검색만 계속
    /// - 쿨타임 끝나면 이 브랜치는 실패 → Combat/Wait 로 넘어감
    /// </summary>
    private Node CreateCooldownTree()
    {
        var cooldownSequence = new SequenceNode();

        // 쿨타임 중일 때만 이 서브트리가 동작
        cooldownSequence.AddChild(new IsCoolingDownNode(blackboard));

        // 제자리에서 계속 타겟 검색 (재진입/새 타겟 가능)
        cooldownSequence.AddChild(new FindTargetNode(blackboard));

        // 쿨타임이 도는 동안 Running, 끝나면 Success
        cooldownSequence.AddChild(new CooldownWaitNode(blackboard));

        return cooldownSequence;
    }

    /// <summary>
    /// 전투 상태:
    /// - 탐지 범위 내 유효 타겟 있으면
    ///   - 쿨타임 아니고 사거리 안이면 공격
    ///   - 아니면 추적
    /// - 타겟 없거나 유효하지 않으면 실패 → Wait 로 이동
    /// </summary>
    private Node CreateCombatTree()
    {
        var combatSequence = new SequenceNode();

        // 항상 먼저 타겟 검색/유지
        combatSequence.AddChild(new FindTargetNode(blackboard));

        // // 타겟이 있어야 전투 가능
        // combatSequence.AddChild(new HasTargetNode(blackboard));
        //
        // // 타겟이 탐지 범위 안에 있어야 전투 유지
        // combatSequence.AddChild(new TargetValidNode(blackboard));

        var combatSelector = new SelectorNode();
        combatSequence.AddChild(combatSelector);

        // 1) 공격 브랜치: 쿨타임 X + 사거리 안
        var attackSequence = new SequenceNode();
        attackSequence.AddChild(new CanAttackNode(blackboard));
        attackSequence.AddChild(new IsInAttackRangeNode(blackboard));
        attackSequence.AddChild(new AttackNode(blackboard));
        combatSelector.AddChild(attackSequence);

        // 2) 추적 브랜치: 공격은 못 하지만(사거리 밖 등) 타겟은 있음
        var chaseSequence = new SequenceNode();
        chaseSequence.AddChild(new MoveToTargetNode(blackboard));
        combatSelector.AddChild(chaseSequence);

        return combatSequence;
    }

    /// <summary>
    /// 대기 상태:
    /// - 타겟을 먼저 검색
    ///   - 찾으면 MoveToWaitPositionNode 가 실패 → Combat 쪽으로 넘어감
    /// - 못 찾으면 대기 위치로 이동하면서 계속 검색
    /// </summary>
    private Node CreateWaitTree()
    {
        var waitSequence = new SequenceNode();

        // 먼저 주변 타겟 검색
        waitSequence.AddChild(new FindTargetNode(blackboard));

        // 타겟이 생기면 Failure 반환 → 상위 Selector 가 Combat 쪽 시도
        // 타겟이 없으면 대기 위치로 이동하면서 Running
        waitSequence.AddChild(new MoveToWaitPositionNode(blackboard));

        return waitSequence;
    }
}