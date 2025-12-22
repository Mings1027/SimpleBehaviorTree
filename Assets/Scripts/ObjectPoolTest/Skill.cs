using UnityEngine;

public class Skill : MonoBehaviour
{
    public void Shoot(GameObject prefab, Transform firePoint, Player player)
    {
        var projectileObj = ObjectPoolManager.Get(prefab, firePoint.position, firePoint.rotation);
        if (projectileObj.TryGetComponent(out Projectile projectile))
        {
            projectile.Init(player, this);
            projectile.OnHit.Bind(projectile, (t, obj) =>
            {
                var skill = t.Skill as SingleSkill;
                skill?.Hit(t.Owner, obj);
            });
        }
    }

    private void Hit(Player player, GameObject obj)
    {
        Debug.Log($"Hit {player.name}");
        Debug.Log($"Hit {obj.name}");
    }
}