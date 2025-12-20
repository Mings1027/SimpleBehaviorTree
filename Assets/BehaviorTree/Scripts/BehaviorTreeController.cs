using UnityEngine;
using BehaviorTree;
using System.Collections.Generic;
using BehaviorTree.Demo.Scripts.EnemyAI;

public class BehaviorTreeController : MonoBehaviour
{
    private Node _rootNode;

#if UNITY_EDITOR
    public Node RootNode => _rootNode;

    // 이번 프레임에 Update()가 호출된 노드들
    public readonly HashSet<Node> UpdatedThisFrame = new();

    // 이번 프레임 각 노드의 마지막 결과
    public readonly Dictionary<Node, NodeState> LastResults = new();
#endif

    private float remainingTime;

    [SerializeField] private float updateInterval = 0.1f;

    [SerializeField] private AIContext blackboard;
    public AIContext Blackboard => blackboard;

    private void OnEnable()
    {
#if UNITY_EDITOR
        Node.OnNodeUpdated += HandleNodeUpdated;
#endif
        remainingTime = updateInterval;
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        Node.OnNodeUpdated -= HandleNodeUpdated;
#endif
    }

    private void Update()
    {
        if (_rootNode == null)
            return;

#if UNITY_EDITOR
        UpdatedThisFrame.Clear();
        LastResults.Clear();
#endif

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0)
        {
            remainingTime = updateInterval;
            _rootNode.Update();
        }
    }

#if UNITY_EDITOR
    private void HandleNodeUpdated(Node node, NodeState result)
    {
        UpdatedThisFrame.Add(node);
        LastResults[node] = result;
    }

    private void OnDrawGizmos()
    {
        _rootNode?.DrawGizmos();
    }
#endif

    public void CreateTree(Node rootNode)
    {
        blackboard.self = transform;
        
        _rootNode = rootNode;
    }
}