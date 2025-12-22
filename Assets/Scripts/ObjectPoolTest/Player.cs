using UnityEngine;

public class Player : UpdateBehaviour
{
    private Skill skill;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        // ObjectPoolManager.Register(prefab, 10, true, 4);
        skill = GetComponent<Skill>();
    }

    public override void OnUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            skill.Shoot(prefab, firePoint, this);
        }
    }
}