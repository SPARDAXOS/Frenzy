using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class MoneyPickup : MonoBehaviour {



    private PickupSpawner parentSpawner;
    public bool spawned = false;

    public int currentPickupID = -1;
    public float respawnTimer = 0.0f;
    public float respawnDuration = 1.0f;
    public bool shouldRespawn = false;


    private BoxCollider2D collider2DRef;
    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;


    public void Awake() {
        collider2DRef = GetComponent<BoxCollider2D>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        animatorRef = GetComponent<Animator>();
    }
    public void Update() {
        if (!spawned && shouldRespawn && respawnTimer > 0.0f) {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0.0f) {
                respawnTimer = 0.0f;
                SetState(true);
            }
        }
    }




    public void SetParentSpawner(PickupSpawner spawner) { parentSpawner = spawner; }
    public void SetState(bool state) {
        spawned = state;
        collider2DRef.enabled = state;
        spriteRendererRef.enabled = state;
        if (animatorRef)
            animatorRef.enabled = state;
    }
    public void SetPickupID(int id) { currentPickupID = id; }
    public void SetShouldRespawn(bool state) { shouldRespawn = state; }
    public int GetPickupID() { return currentPickupID; }
    public bool GetState() { return spawned; }
    public void SetRespawnDelay(float duration) { respawnDuration = duration; }

    public void Despawn() {
        SetState(false);
        if (shouldRespawn)
            respawnTimer = respawnDuration;
    }


    private void OnTriggerEnter2D(Collider2D collision) {

        Player playerComponent = collision.GetComponent<Player>();
        if (playerComponent) {
            if (Netcode.IsHost()) {
                playerComponent.RegisterMoneyPickup(currentPickupID, 1);
                Despawn();
            }
            else
                return;
        }
    }


}
