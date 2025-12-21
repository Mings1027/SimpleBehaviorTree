using UnityEngine;

public class Player : UpdateBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform firePoint;

    private void Awake()
    {
        // ObjectPoolManager.Register(prefab, 10, true, 4);
    }

    public override void OnUpdate()
    {
        if (Input.GetKey(KeyCode.Space))
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
        ObjectPoolManager.Get(prefab, firePoint.position, firePoint.rotation);
    }

    private void Delete() { }
}