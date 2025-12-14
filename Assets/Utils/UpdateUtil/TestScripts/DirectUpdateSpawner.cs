using UnityEngine;

public class DirectUpdateSpawner : MonoBehaviour
{
    [SerializeField] private int count = 100;

    private void Start()
    {
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"Direct_{i}");
            go.AddComponent<DirectUpdateTest>();
        }
    }
}