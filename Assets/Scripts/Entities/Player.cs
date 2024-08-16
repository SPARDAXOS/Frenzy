using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;
using static MyUtility.Utility;

public class Player : Entity {

    //QuickHack
    public static int PLAYER_1_MONEYDROP_ID = -2;
    public static int PLAYER_2_MONEYDROP_ID = -3;

    [Header("Health")]
    [Space(10)]
    [SerializeField] private float healthCap = 100.0f;
    [SerializeField] private float startingHealth = 100.0f;
    [SerializeField] private float respawnDelay = 5.0f;

    [Header("Drop")]
    [Space(10)]
    [SerializeField] private GameObject moneyDropPrefab;
    [SerializeField] private Vector2 moneyDropOffset = new Vector2(5.0f, 2.0f);

    [Header("Shooting")]
    [Space(10)]
    [SerializeField] private GameObject projectilePoolPrefab = null;
    [SerializeField] private float fireRateDelay = 0.2f;

    [SerializeField] private Vector2 bulletOffset = new Vector2(5.0f, 2.0f);
    [SerializeField] private float bulletOffsetYDeviation = 0.1f;

    [Header("Movement")]
    [Space(10)]
    [SerializeField] private float accelerationSpeed = 1.0f;
    [SerializeField] private float decelerationSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 100.0f;

    [Header("Jump")]
    [Space(10)]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float jumpHeight = 5.0f;
    [SerializeField] private float jumpGravityPower = 50.0f;
    [SerializeField] private float jumpBufferDuration = 0.3f;

    private float jumpBufferTimer = 0.0f;
    private float respawnTimer = 0.0f;
    private float fireRateTimer = 0.0f;

    public bool isDead = false;
    public bool isGrounded = true;



    public enum PlayerID {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    private PlayerID currentPlayerID = PlayerID.NONE;
    private bool active = true;

    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;
    private Rigidbody2D rigidbody2DRef;
    private BoxCollider2D boxCollider2DRef;
    private NetworkObject networkObjectRef;

    private GameObject projectilesPoolRef;
    private GameObject playerGUIRef;
    private GameObject moneydrop;

    private MoneyPickup moneyDropScript;
    private TMP_Text respawnTextComp;
    private MainHUD mainHUDRef;
    private ProjectilePool projectilesPoolScript;

    public float currentHealth = 0.0f;
    public float currentSpeed = 0.0f;
    public int currentMoney = 0;

    public Vector2 inputDirection = Vector2.zero;
    public float horizontalVelocity = 0.0f;
    public float verticalVelocity = 0.0f;

    public bool movingRight = false;
    public bool movingLeft = false;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        spriteRendererRef = GetComponent<SpriteRenderer>();
        animatorRef = GetComponent<Animator>();
        rigidbody2DRef = GetComponent<Rigidbody2D>();
        boxCollider2DRef = GetComponent<BoxCollider2D>();
        networkObjectRef = GetComponent<NetworkObject>();

        SetupReferences();
        CreateMoneyDrop();
        SetupProjectilesPool();

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized || !active)
            return;

        if (currentPlayerID == PlayerID.NONE)
            return;

        if (isDead) {
            UpdateRespawnTimer();
            return;
        }


        UpdateFireRateTimer();
        if (networkObjectRef.IsOwner)
            CheckInput();

        UpdateGroundedCheck();
        UpdateJumpBuffer();
        CheckJumpCommand();
    }
    public override void FixedTick() {
        if (!initialized || !active)
            return;

        if (currentPlayerID == PlayerID.NONE)
            return;

        if (isDead)
            return;


        if (networkObjectRef.IsOwner) {
            if (Netcode.IsClient() && !Netcode.IsHost()) {
                gameInstanceRef.GetRPCManagement().CalculatePlayer2PositionServerRpc(inputDirection.x);
            }
        }

        UpdateSpeed();
        UpdateMovement();
    }
    public void SetupStartingState() {
        if (!initialized)
            return;

        currentHealth = startingHealth;
        currentMoney = 0;
        isDead = false;
        animatorRef.SetBool("isDead", false);
        respawnTextComp.gameObject.SetActive(false);
        fireRateTimer = 0.0f;
        //Hide Timer, Update anim, send rpc for anim

        rigidbody2DRef.constraints = RigidbodyConstraints2D.FreezeRotation;

        //To func?
        mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);
        mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
        //Health reset, etc
    }
    public void SetNetworkedEntityState(bool state) {
        active = state;
        if (state) {
            spriteRendererRef.enabled = true;
            rigidbody2DRef.WakeUp();
            boxCollider2DRef.enabled = true;
            animatorRef.enabled = true;
        }
        else if (!state) {
            spriteRendererRef.enabled = false;
            rigidbody2DRef.Sleep();
            boxCollider2DRef.enabled = false;
            animatorRef.enabled = false;
        }
    }
    private void SetupProjectilesPool() {
        if (!projectilePoolPrefab)
            return;

        projectilesPoolRef = Instantiate(projectilePoolPrefab);
        projectilesPoolRef.name = gameObject.name + "_ProjectilesPool";
        projectilesPoolScript = projectilesPoolRef.GetComponent<ProjectilePool>();
        Validate(projectilesPoolScript, "Failed to find ProjectilesPool component reference!", ValidationLevel.ERROR, true);
        projectilesPoolScript.Setup(currentPlayerID);
    }
    private void SetupReferences() {

        //Respawn Text
        Transform playerGUITransform = transform.Find("PlayerGUI");
        Validate(playerGUITransform, "Failed to find PlayerGUI reference!", ValidationLevel.ERROR, true);
        playerGUIRef = playerGUITransform.gameObject;

        Transform respawnTextTransform = playerGUITransform.Find("RespawnText");
        Validate(respawnTextTransform, "Failed to find RespawnText reference!", ValidationLevel.ERROR, true);
        respawnTextComp = respawnTextTransform.GetComponent<TMP_Text>();
        Validate(respawnTextComp, "Failed to find respawnTextComp reference!", ValidationLevel.ERROR, true);

        respawnTextComp.gameObject.SetActive(false);
    }
    private void CreateMoneyDrop() {
        Validate(moneyDropPrefab, "Failed to find moneyDropPrefab reference!", ValidationLevel.ERROR, true);
        
        moneydrop = Instantiate(moneyDropPrefab);
        moneydrop.name = gameObject.name + "_MoneyDrop";
        moneyDropScript = moneydrop.GetComponent<MoneyPickup>();
        moneyDropScript.SetShouldRespawn(false);
        if (currentPlayerID == PlayerID.PLAYER_1)
            moneyDropScript.SetPickupID(PLAYER_1_MONEYDROP_ID);
        else if (currentPlayerID == PlayerID.PLAYER_2)
            moneyDropScript.SetPickupID(PLAYER_2_MONEYDROP_ID);

        moneyDropScript.SetState(false);
    }


    private void Accelerate() {
        if (currentSpeed >= maxSpeed)
            return;

        currentSpeed += accelerationSpeed * Time.fixedDeltaTime;
        if (currentSpeed >= maxSpeed) {
            currentSpeed = maxSpeed;

        }
    }
    private void Decelerate() {
        if (currentSpeed <= 0.0f)
            return;

        currentSpeed -= decelerationSpeed * Time.fixedDeltaTime;
        if (currentSpeed <= 0.0f) {
            currentSpeed = 0.0f;

        }
    }
    private void CheckInput() {
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);
        bool jump = Input.GetKeyDown(KeyCode.W);
        bool shoot = Input.GetKey(KeyCode.Space);



        if (left && right)
            inputDirection.x = 0.0f;
        else if (left)
            inputDirection.x = -1.0f;
        else if (right)
            inputDirection.x = 1.0f;
        else
            inputDirection.x = 0.0f;

        UpdateSpriteOrientation(inputDirection.x);
        

        if (Netcode.IsHost()) {
            movingLeft = left;
            movingRight = right;
            if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
                SetMovementAnimationState(true);
            else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
                SetMovementAnimationState(false);

            if (jump)
                jumpBufferTimer = jumpBufferDuration;

            if (shoot && projectilesPoolScript) {
                if (ShootProjectile(false))
                    gameInstanceRef.GetRPCManagement().NotifyShootRequestServerRpc(Netcode.GetClientID());
            }
        }
        else if (Netcode.IsClient() && !Netcode.IsHost()) {

            if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
                gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(true);
            else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
                gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(false);

            if (jump)
                gameInstanceRef.GetRPCManagement().NotifyPlayer2JumpCommandServerRpc();

            if (shoot && projectilesPoolScript) {
                if (ShootProjectile(true))
                    gameInstanceRef.GetRPCManagement().NotifyShootRequestServerRpc(Netcode.GetClientID());
            }
        }
    }
    public bool ShootProjectile(bool consmeticOnly) {
        if (fireRateTimer > 0.0f)
            return false;

        Vector2 shootingDirection = Vector2.zero;
        if (spriteRendererRef.flipX)
            shootingDirection.x = -1.0f;
        else
            shootingDirection.x = 1.0f;

        Vector2 shootingPosition = transform.position;
        shootingPosition.y += bulletOffset.y;
        shootingPosition.y += UnityEngine.Random.Range(-bulletOffsetYDeviation, bulletOffsetYDeviation);
        shootingPosition.x += bulletOffset.x * shootingDirection.x;

        projectilesPoolScript.SpawnProjectile(shootingPosition, shootingDirection, consmeticOnly);
        fireRateTimer = fireRateDelay;
        gameInstanceRef.GetSoundSystem().PlaySFX("Shoot", true, gameObject);
        return true;
    }
    public void SetMoneyDropState(bool state) {
        moneyDropScript.SetState(state);
    }

    private void UpdateRespawnTimer() {
        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0.0f) {
            respawnTimer = 0.0f;
            gameInstanceRef.GetSoundSystem().PlaySFX("Respawn");
            SetupStartingState();
        }

        respawnTextComp.text = Mathf.RoundToInt(respawnTimer).ToString();
    }
    private void UpdateFireRateTimer() {
        if (fireRateTimer > 0.0f) {
            fireRateTimer -= Time.deltaTime;
            if (fireRateTimer <= 0.0f)
                fireRateTimer = 0.0f;
        }
    }
    private void UpdateJumpBuffer() {
        if (jumpBufferTimer > 0.0f) {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0.0f)
                jumpBufferTimer = 0.0f;
        }
    }
    private void CheckJumpCommand() {
        if (jumpBufferTimer > 0.0f && isGrounded) {
            jumpBufferTimer = 0.0f;
            Jump();
        }
    }



    private void Jump() {
        rigidbody2DRef.gravityScale = jumpGravityPower; //Repeated here since it is required for the below calculation
        float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * jumpGravityPower));
        rigidbody2DRef.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
    }
    public void SetMovementAnimationState(bool state) {
        animatorRef.SetBool("isMoving", state);
    }


    private void UpdateSpeed() {
        if (movingRight || movingLeft)
            Accelerate();
        else
            Decelerate();
    }
    private void UpdateMovement() {
        horizontalVelocity = (inputDirection * currentSpeed).x;
        verticalVelocity = rigidbody2DRef.velocity.y; //? weird spot
        rigidbody2DRef.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }
    private void UpdateSpriteOrientation(float input) {
        if (input == 0.0f)
            return;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        if (input > 0.0f && spriteRendererRef.flipX) {
            spriteRendererRef.flipX = false;
            management.UpdateSpriteOrientationServerRpc(spriteRendererRef.flipX, Netcode.GetClientID());
        }
        else if (input < 0.0f && !spriteRendererRef.flipX) {
            spriteRendererRef.flipX = true;
            management.UpdateSpriteOrientationServerRpc(spriteRendererRef.flipX, Netcode.GetClientID());
        }
    }
    private void UpdateGroundedCheck() {

        Vector2 position = transform.position;
        position.y += 0.05f;
        RaycastHit2D results 
            = Physics2D.BoxCast(position, boxCollider2DRef.size, 0.0f, Vector2.down, 0.2f, groundLayerMask.value);
        
        if (results) {
            isGrounded = true;
            if (isDead)
                rigidbody2DRef.constraints = RigidbodyConstraints2D.FreezePositionY;
        }
        else {
            isGrounded = false;

        }
    }

    public void OverrideCurrentHealth(float percentage) { currentHealth = percentage * healthCap;}
    public void OverrideCurrentMoney(int amount) { currentMoney = amount; }
    public bool RegisterMoneyPickup(int pickupID, int amount) {
        if (!Netcode.IsHost())
            return false;
        if (isDead)
            return false;

        currentMoney += amount;
        mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.UpdatePlayerMoneyServerRpc(currentMoney, currentPlayerID, Netcode.GetClientID());
        management.UpdatePickupSpawnServerRpc(pickupID, Netcode.GetClientID());
        gameInstanceRef.GetSoundSystem().PlaySFX("MoneyCollect", true, gameObject);

        return true;
    }
    public void TakeDamage(float damage) {
        if (isDead)
            return;

        if (damage == 0.0f)
            return;

        float value = damage;
        if (value < 0.0f)
            value *= -1.0f;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        currentHealth -= value;
        if (currentHealth <= 0.0f) {
            currentHealth = 0.0f;
            management.UpdatePlayerDeathServerRpc(currentPlayerID, Netcode.GetClientID());
            Kill();
        }
        mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);

        management.UpdatePlayerHealthServerRpc(GetCurrentHealthPercentage(), currentPlayerID, Netcode.GetClientID());
    }
    public void Kill() {
        if (isDead)
            return;

        isDead = true;
        animatorRef.SetTrigger("deathTrigger");
        animatorRef.SetBool("isDead", true);
        respawnTimer = respawnDelay;
        respawnTextComp.gameObject.SetActive(true);
        respawnTextComp.text = Mathf.RoundToInt(respawnTimer).ToString();
        gameInstanceRef.GetSoundSystem().PlaySFX("Death");

        if (currentMoney > 0) {
            Vector2 playerDirection = Vector2.zero;
            if (spriteRendererRef.flipX)
                playerDirection.x = -1.0f;
            else
                playerDirection.x = 1.0f;

            Vector2 shootingPosition = transform.position;
            shootingPosition.y += moneyDropOffset.y;
            shootingPosition.x += moneyDropOffset.x * playerDirection.x;

            moneyDropScript.transform.position = shootingPosition;
            moneyDropScript.OverrideAmount(currentMoney);

            moneyDropScript.SetState(true);
        }


        currentMoney = 0;
        mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.UpdatePlayerMoneyServerRpc(currentMoney, currentPlayerID, Netcode.GetClientID());
    }

    public void ProcessSpriteOrientationRpc(bool flipX) {
        spriteRendererRef.flipX = flipX;
    }
    public void ProcessMovementInputRpc(float input) {

        inputDirection.x = input;
        if (input == 0.0f) {
            movingLeft = false;
            movingRight = false;
        }
        else if (input < 0.0f) {
            movingLeft = true;
            movingRight = false;
        }
        else if (input > 0.0f) {
            movingLeft = false;
            movingRight = true;
        }
    }
    public void ProcessJumpInputRpc() {
        jumpBufferTimer = jumpBufferDuration;
    }



    public PlayerID GetPlayerID() { return currentPlayerID; }
    public void SetPlayerID(PlayerID id) { currentPlayerID = id; }
    public void SetMainHUDRef(MainHUD reference) { mainHUDRef = reference; }
    public int GetCurrentMoney() { return currentMoney; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return currentHealth / healthCap; }
    public ProjectilePool GetProjectilePool() { return projectilesPoolScript; }
}
