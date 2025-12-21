using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Pooling/PoolRegistry")]
public class PoolRegistry : ScriptableObject
{
    private Dictionary<GameObject, PoolEntry> _lookupTable = new();
    private bool _isInit;

    public List<PoolEntry> Entries => entries;

    [SerializeField] private List<PoolEntry> entries = new();

    private void Init()
    {
        if (_isInit) return;

        _lookupTable = new Dictionary<GameObject, PoolEntry>();
        foreach (var entry in entries)
        {
            if (entry.Prefab == null) continue;
            if (!_lookupTable.ContainsKey(entry.Prefab))
            {
                _lookupTable.Add(entry.Prefab, entry);
            }
            else
            {
                Debug.LogWarning($"[PoolRegistry] Duplicate prefab found: {entry.Prefab.name}. Ignoring duplicate.");
            }
        }

        _isInit = true;
    }

    public bool TryGetEntry(GameObject prefab, out PoolEntry entry)
    {
        if (!_isInit) Init();

        if (prefab == null)
        {
            entry = default;
            return false;
        }

        return _lookupTable.TryGetValue(prefab, out entry);
    }
}

[System.Serializable]
public struct PoolEntry
{
    public GameObject Prefab;
    public int PreloadCount;
    public bool UseAutoRelease;
    public float AutoReleaseDelay;
}