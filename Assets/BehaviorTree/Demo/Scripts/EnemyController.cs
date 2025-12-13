using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Enemy enemy;

    private void Start()
    {
        enemy.onHit.Bind(this, (t, obj) => t.OnEnemyHit(obj));
        enemy.onDeath.Bind(this, t => t.OnEnemyDeath());
    }

    private void OnEnemyHit(GameObject obj)
    {
        Debug.Log($"{enemy.name} Hit by {obj?.name}");
    }

    private void OnEnemyDeath()
    {
        Debug.Log($"{enemy.name} Dead!");
    }
}