using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour {

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectilePoolSize = 20;

    public Player.PlayerID ownerPlayerID = Player.PlayerID.NONE;
    public List<Projectile> projectilesPool = new List<Projectile>();
    public bool valid = false;

    public void Setup(Player.PlayerID id) {
        if (id == Player.PlayerID.NONE)
            return;

        if (!CreateProjectilesPool(id))
            return;

        ownerPlayerID = id;
        valid = true;
    }
    private bool CreateProjectilesPool(Player.PlayerID id) {
        if (projectilePoolSize == 0 || !projectilePrefab)
            return false;

        for (int i = 0; i < projectilePoolSize; i++) {
            GameObject newProjectile = Instantiate(projectilePrefab);
            newProjectile.name = "Projectile_" + i;
            newProjectile.transform.SetParent(transform);
            Projectile projectileComp = newProjectile.GetComponent<Projectile>();
            projectileComp.SetParentPool(this);
            projectileComp.SetOwnerPlayerID(id);
            projectileComp.SetState(false);

            projectilesPool.Add(projectileComp);
        }

        return true;
    }
    public bool SpawnProjectile(Vector3 position, Vector2 direction, bool cosmeticOnly) {
        if (projectilesPool.Count == 0)
            return false;

        foreach (var projectile in projectilesPool) {
            if (!projectile.GetState()) {
                projectile.Spawn(position, direction, cosmeticOnly);
                return true;
            }
        }

        return true;
    }


}
