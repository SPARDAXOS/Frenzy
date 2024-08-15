using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Projectile : MonoBehaviour {


    [SerializeField] public float speed = 300.0f;
    [SerializeField] public float damage = 20.0f;

    public bool spawned = false;
    public bool cosmeticOnly = false;
    public Player.PlayerID ownerPlayerID = Player.PlayerID.NONE;
    private ProjectilePool parentPool = null;

    public float cachedLightIntensity = 0.0f;
    public Vector2 direction = Vector2.zero;


    private CircleCollider2D collider2DRef;
    private SpriteRenderer spriteRendererRef;
    private Light2D light2DRef;
    private Rigidbody2D rigidbody2DRef;

    private void Awake() {
        collider2DRef = GetComponent<CircleCollider2D>();
        spriteRendererRef = GetComponent<SpriteRenderer>();
        light2DRef = GetComponent<Light2D>();
        rigidbody2DRef = GetComponent<Rigidbody2D>();
        cachedLightIntensity = light2DRef.intensity;
    }
    void Update() {
        if (spawned) {
            rigidbody2DRef.velocity = direction * speed * Time.deltaTime;
        }
        
    }
    public void SetParentPool(ProjectilePool parent) { parentPool = parent; }
    public void SetOwnerPlayerID(Player.PlayerID id) { ownerPlayerID = id; }

    public void SetState(bool state) {
        spawned = state;
        collider2DRef.enabled = state;
        spriteRendererRef.enabled = state;
        if (state)
            light2DRef.intensity = cachedLightIntensity;
        else
            light2DRef.intensity = 0.0f;
    }
    public void Spawn(Vector3 position, Vector2 direction, bool cosmeticOnly) {

        transform.position = position;
        this.direction = direction;
        this.cosmeticOnly = cosmeticOnly;
        SetState(true);
    }
    public bool GetState() { return spawned; }
    public void Despawn() {
        SetState(false);
    }



    private void OnTriggerEnter2D(Collider2D collision) {
        if (ownerPlayerID == Player.PlayerID.NONE)
            Despawn();

        Player playerComponent = collision.GetComponent<Player>();
        if (playerComponent) {
            if (playerComponent.GetPlayerID() == ownerPlayerID)
                return;

            if (!cosmeticOnly)
                playerComponent.TakeDamage(damage);
            
        }

        Despawn();
    }
}
