using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.UIElements;
using static MyUtility.Utility;

public class Player : Entity {

    [Header("Health")]
    [Space(10)]
    [SerializeField] private float healthCap = 100.0f;
    [SerializeField] private float startingHealth = 100.0f;

    [Header("Shooting")]
    [Space(10)]
    [SerializeField] private GameObject projectilePoolPrefab = null;
    [SerializeField] private Vector2 bulletOffset = new Vector2(5.0f, 2.0f);

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
    [SerializeField] [Range(0.0f, 1.0f)] private float jumpCancelPercentage = 0.6f;

    public float jumpBufferTimer = 0.0f;

    public bool isGrounded = true;


    //private void CheckVerticalVelocity() {
    //    if (rigidbodyComp.velocity.y < 0.0f) {
    //        if (coyoteTimeTimer == 0.0f && isGrounded && !isDead)
    //            coyoteTimeTimer = coyoteTimeDuration;

    //        if (isDead && !isGrounded) {
    //            rigidbodyComp.gravityScale = airDeathFallingGravity;
    //        }

    //        if (performingMeleeCommand)
    //            StopMeleeCommand();
    //        if (performingShootCommand)
    //            StopShootCommand();
    //    }
    //    else if (rigidbodyComp.velocity.y > 0.0f) {
    //        isGrounded = false;
    //        animatorComp.SetBool("isGrounded", false);
    //        animatorComp.SetBool("isFalling", false);
    //        coyoteTimeTimer = 0.0f;
    //        rigidbodyComp.gravityScale = raisingGravity;

    //        Vector3 gunshotLightPosition = gunshotLight.transform.localPosition;
    //        gunshotLightPosition.y = gunshotLightInAirHeight;
    //        gunshotLight.transform.localPosition = gunshotLightPosition;

    //        Vector3 bulletCasingPosition = bulletCasing.transform.localPosition;
    //        bulletCasingPosition.y = bulletCasingInAirHeight;
    //        bulletCasing.transform.localPosition = bulletCasingPosition;
    //    }
    //    else if (rigidbodyComp.velocity.y == 0.0f) {
    //        if (!isDead) {
    //            rigidbodyComp.gravityScale = groundedGravity;
    //            if (!isGrounded) {
    //                pushbackActive = false;
    //                rigidbodyComp.velocity = new Vector2(velocityLastFrame.x * landingRetainedVelocity, 0.0f);

    //                QueueCameraShake(landingCameraShake);
    //                ApplyHitStop(landingHitStopDuration);
    //            }
    //        }

    //        isGrounded = true;
    //        animatorComp.SetBool("isGrounded", true);

    //        coyoteTimeTimer = 0.0f;


    //        Vector3 gunshotLightPosition = gunshotLight.transform.localPosition;
    //        gunshotLightPosition.y = gunshotLightGroundedHeight;
    //        gunshotLight.transform.localPosition = gunshotLightPosition;

    //        Vector3 bulletCasingPosition = bulletCasing.transform.localPosition;
    //        if (isMoving && isGrounded)
    //            bulletCasingPosition.y = bulletCasingRunningHeight;
    //        else
    //            bulletCasingPosition.y = bulletCasingGroundedHeight;
    //        bulletCasing.transform.localPosition = bulletCasingPosition;
    //    }

    //    velocityLastFrame = rigidbodyComp.velocity;
    //}


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
    private MainHUD mainHUDRef;
    private GameObject projectilesPoolRef;
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

        SetupProjectilesPool();

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized || !active)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;

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
        bool shoot = Input.GetKeyDown(KeyCode.Space);



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
                ShootProjectile(false);
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
                ShootProjectile(true);
                gameInstanceRef.GetRPCManagement().NotifyShootRequestServerRpc(Netcode.GetClientID());
            }
        }
    }
    public void ShootProjectile(bool consmeticOnly) {
        Vector2 shootingDirection = Vector2.zero;
        if (spriteRendererRef.flipX)
            shootingDirection.x = -1.0f;
        else
            shootingDirection.x = 1.0f;

        Vector2 shootingPosition = transform.position;
        shootingPosition.y += bulletOffset.y;
        shootingPosition.x += bulletOffset.x * shootingDirection.x;

        projectilesPoolScript.SpawnProjectile(shootingPosition, shootingDirection, consmeticOnly);
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

        }
        else {
            isGrounded = false;

        }
    }


    public void RegisterMoneyPickup(int pickupID, int amount) {
        if (!Netcode.IsHost())
            return;

        currentMoney += amount;
        mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.UpdatePlayerMoneyServerRpc(currentMoney, currentPlayerID, Netcode.GetClientID());
        management.UpdatePickupSpawnServerRpc(pickupID, Netcode.GetClientID());
    }
    public void TakeDamage(float damage) {
        if (damage == 0.0f)
            return;
        float value = damage;
        if (value < 0.0f)
            value *= -1.0f;


        currentHealth -= value;
        if (currentHealth <= 0.0f) {
            currentHealth = 0.0f;
            //Death then delay to respawn. delay is set to 0 along with death bool at setupstartstate
            //Send rpc to signal death.
        }
        mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.UpdatePlayerHealthServerRpc(GetCurrentHealthPercentage(), currentPlayerID, Netcode.GetClientID());
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
    public void ProcessPlayerHealthRpc(float amount) {
        currentHealth = amount;
        //Check death?
        //This is incorrect! Im getting player 2s health sent to me. Process should be broken in 2. Value to just update?? idk
        if (currentPlayerID == PlayerID.PLAYER_1)
            mainHUDRef.UpdatePlayerHealth(amount, Player.PlayerID.PLAYER_2);
        else if (currentPlayerID == PlayerID.PLAYER_1)
            mainHUDRef.UpdatePlayerHealth(amount, Player.PlayerID.PLAYER_1);
    }

    //Probably need to update not only the hud but the value here too!
    //So the game instance calls this here which sets the value then updates the hud from here!


    public PlayerID GetPlayerID() { return currentPlayerID; }
    public void SetPlayerID(PlayerID id) { currentPlayerID = id; }
    public void SetMainHUDRef(MainHUD reference) { mainHUDRef = reference; }
    public float GetCurrentHealth() { return currentHealth; }
    public float GetCurrentHealthPercentage() { return currentHealth / healthCap; }
    public ProjectilePool GetProjectilePool() { return projectilesPoolScript; }

}
