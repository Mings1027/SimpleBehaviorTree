using UnityEngine;

public class ManagedUpdateSpawner : MonoBehaviour
{
    [SerializeField] private int count = 100;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Managed_{i}");
            go.AddComponent<ManagedUpdateTest>();
        }
    }
}