using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MoneyPickup : MonoBehaviour {


    [SerializeField] private int amount = 1;

    private PickupSpawner parentSpawner;
    public bool spawned = false;

    public int currentPickupID = -1;
    public float respawnTimer = 0.0f;
    public float respawnDuration = 1.0f;
    public bool shouldRespawn = false;
    private float chachedLightIntensity = 0.0f;


    private BoxCollider2D collider2DRef;
    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;
    private Light2D light2DRef;


    public void Awake() {
        collider2DRef = GetComponent<BoxCollider2D>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        animatorRef = GetComponent<Animator>();
        light2DRef = GetComponent<Light2D>();
        chachedLightIntensity = light2DRef.intensity;
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



    public void OverrideAmount(int amount) { this.amount = amount; }
    public void SetParentSpawner(PickupSpawner spawner) { parentSpawner = spawner; }
    public void SetState(bool state) {
        spawned = state;
        collider2DRef.enabled = state;
        spriteRendererRef.enabled = state;
        if (animatorRef)
            animatorRef.enabled = state;
        if (state)
            light2DRef.intensity = chachedLightIntensity;
        else
            light2DRef.intensity = 0.0f;
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
                if (playerComponent.RegisterMoneyPickup(currentPickupID, amount))
                    Despawn();
            }
            else
                return;
        }
    }


}
