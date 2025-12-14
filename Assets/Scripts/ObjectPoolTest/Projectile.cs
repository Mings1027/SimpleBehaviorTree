using UnityEngine;
using Utils;

public class Projectile : UpdateBehaviour
{
    [SerializeField] private float speed = 10;

    public override void OnUpdate()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }
}