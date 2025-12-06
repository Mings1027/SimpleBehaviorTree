using System;
using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeController : MonoBehaviour
    {
        private Node _rootNode;
        public Node RootNode => _rootNode;

        public void Update()
        {
            _rootNode?.Update();
        }

        private void OnDrawGizmos()
        {
            _rootNode?.DrawGizmos();
        }

        public void CreateTree(Node rootNode)
        {
            _rootNode = rootNode;
        }
    }
}