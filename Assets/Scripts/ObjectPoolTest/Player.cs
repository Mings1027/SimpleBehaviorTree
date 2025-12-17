using UnityEngine;

public class Player : UpdateBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        ObjectPoolManager.Register(prefab, 10);
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            Delete();
        }
    }

    private void Shoot()
    {
        var projectile = ObjectPoolManager.Get(prefab, false);
        projectile.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        projectile.SetActive(true);
    }

    private void Delete()
    {
    }
}