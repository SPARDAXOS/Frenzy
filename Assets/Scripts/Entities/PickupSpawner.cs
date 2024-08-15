using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MyUtility.Utility;




public class PickupSpawner : MonoBehaviour {

    static int PICKUP_ID = 0;

    [SerializeField] private GameObject moneyPickupPrefab;
    [SerializeField] private bool startWithFullSpawns = true;
    [SerializeField] private bool respawnablePickups = true;
    [SerializeField] private float pickupRespawnDuration = 3.0f;
    [SerializeField] private float pickupRespawnDeviation = 1.0f;

    public List<MoneyPickup> pickupsPool = new List<MoneyPickup>();
    public List<Transform> pickupSpawnPoints = new List<Transform>();
    public int pickupPoolSize = 0;

    private GameObject pickupsPoolParent;
    private GameObject spawnPointsParent;



    private void Start() {
        SetupReferences();
        CreatePickupsPool();
    }
    private void SetupReferences() {
        spawnPointsParent = transform.Find("SpawnPointsParent").gameObject;
        if (!Validate(spawnPointsParent, "Failed to find reference to spawn points parent.", ValidationLevel.WARNING, false))
            return;

        spawnPointsParent.transform.GetComponentsInChildren<Transform>(true, pickupSpawnPoints);
        pickupSpawnPoints.RemoveAt(0); //Removes parent.
        pickupPoolSize = pickupSpawnPoints.Count;
    }
    public bool DeactivatePickup(int id) {
        if (pickupsPool.Count == 0 || id == -1)
            return false;

        foreach(var pickup in pickupsPool) {
            if (pickup.GetPickupID() == id) {
                pickup.Despawn();
                return true;
            }
        }

        //called on player 2 by server to signal to player 2 that they picked up. Pickups will not be picked up by player 2 unless is host.
        return false;
    }
    private void CreatePickupsPool() {
        if (pickupPoolSize == 0 || !moneyPickupPrefab)
            return;

        pickupsPoolParent = new GameObject(gameObject.name + "_PickupsPool");
        for (int i = 0; i < pickupPoolSize; i++) {
            GameObject newPickup = Instantiate(moneyPickupPrefab);
            newPickup.name = "MoneyPickup_" + i;
            MoneyPickup moneyPickupComp = newPickup.GetComponent<MoneyPickup>();
            moneyPickupComp.SetParentSpawner(this);
            moneyPickupComp.SetPickupID(PICKUP_ID);
            moneyPickupComp.SetShouldRespawn(respawnablePickups);
            moneyPickupComp.SetRespawnDelay(pickupRespawnDuration + Random.Range(-pickupRespawnDeviation, pickupRespawnDeviation));


            if (startWithFullSpawns) {
                moneyPickupComp.SetState(true);
                newPickup.transform.position = pickupSpawnPoints[i].position;
            }
            else
                moneyPickupComp.SetState(false);

            newPickup.transform.SetParent(pickupsPoolParent.transform);
            pickupsPool.Add(moneyPickupComp);

            PICKUP_ID++;
        }
    }
}
