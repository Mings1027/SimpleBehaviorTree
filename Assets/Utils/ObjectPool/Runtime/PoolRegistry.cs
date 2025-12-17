using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pooling/PoolRegistry")]
public class PoolRegistry : ScriptableObject
{
    public List<PoolEntry> Entries = new();
}

[System.Serializable]
public struct PoolEntry
{
    public GameObject Prefab;
    public int PreloadCount;
    public bool UseAutoRelease;
    public float AutoReleaseDelay;
}