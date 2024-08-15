using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static MyUtility.Utility;

public class Player : Entity {

    [Header("Health")]
    [Space(10)]
    [SerializeField] private float healthCap = 100.0f;
    [SerializeField] private float startingHealth = 20.0f;

    [Header("Movement")]
    [Space(10)]
    [SerializeField] private float accelerationSpeed = 1.0f;
    [SerializeField] private float decelerationSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 100.0f;

    [Header("Jump")]
    [Space(10)]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float jumpMaximumHeight = 400.0f;
    [SerializeField] private float groundedGravity = 40.0f;
    [SerializeField] private float raisingGravity = 350.0f;
    [SerializeField] private float fallingGravity = 600.0f;
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    //[SerializeField][Range(0.0f, 1.0f)] private float releaseRetainedVelocity = 0.85f;
    //[SerializeField][Range(0.0f, 1.0f)] private float landingRetainedVelocity = 1.0f;
    //[SerializeField] private float jumpBufferDuration = 0.3f;

    private float coyoteTimeTimer = 0.0f;
    private float jumpBufferTimer = 0.0f;

     public bool isGrounded = true;

    //CheckCoyoteTime();
    //CheckJumpCommandBuffer();
    //CheckJumpCommand();

    //private void Jump() {
    //    //For more consistent jump
    //    //It is not 100% accurate unit wise but is close enough
    //    //Raising gravity will effect it slighty.
    //    //For each increase in raisingGravity by 10.0f, reached height is increased by around 1 each time.
    //
    //    rigidbodyComp.gravityScale = raisingGravity; //Repeated here since it is required for the below calculation
    //    float jumpForce = Mathf.Sqrt(jumpMaximumHeight * -2 * (Physics2D.gravity.y * rigidbodyComp.gravityScale));
    //    rigidbodyComp.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
    //    animatorComp.SetTrigger("isJumping");
    //}

    //private void CheckJumpCommand() {
    //    if (!canInterruptMelee && performingMeleeCommand)
    //        return;

    //    if (!canInterruptShooting && performingShootCommand)
    //        return;


    //    if (jumpBufferTimer > 0.0f && isGrounded && !isCrouched) {
    //        jumpBufferTimer = 0.0f;
    //        Jump();
    //    }
    //    if (inputY != -1 && !isGrounded && rigidbodyComp.velocity.y > 0.0f) {
    //        if (releaseRetainedVelocity <= 0.0f)
    //            rigidbodyComp.velocity = new Vector2(rigidbodyComp.velocity.x, -1.0f);
    //        else
    //            rigidbodyComp.velocity = new Vector2(rigidbodyComp.velocity.x, rigidbodyComp.velocity.y * releaseRetainedVelocity);

    //        rigidbodyComp.gravityScale = fallingGravity;
    //    }
    //}
    //private void CheckJumpCommandBuffer() {
    //    if (jumpBufferTimer > 0.0f) {
    //        jumpBufferTimer -= Time.deltaTime;
    //        if (jumpBufferTimer <= 0.0f)
    //            jumpBufferTimer = 0.0f;
    //    }
    //    if (Input.GetKeyDown(KeyCode.UpArrow)) {
    //        jumpBufferTimer = jumpBufferDuration;
    //    }
    //}

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
    //private void CheckCoyoteTime() {
    //    if (coyoteTimeTimer > 0.0f) {
    //        coyoteTimeTimer -= Time.deltaTime;
    //        if (coyoteTimeTimer <= 0.0f) {
    //            coyoteTimeTimer = 0.0f;
    //            isGrounded = false;

    //            rigidbodyComp.gravityScale = fallingGravity;
    //            animatorComp.SetBool("isFalling", true);
    //            animatorComp.SetBool("isGrounded", false);
    //        }
    //    }
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

    public float currentHealth = 0.0f;
    public float currentSpeed = 0.0f;
    public int currentMoney = 0;

    public Vector2 inputDirection = Vector2.zero;
    public Vector2 velocity = Vector2.zero;

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
    }
    public override void FixedTick() {
        if (!initialized || !active)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;


        if (networkObjectRef.IsOwner) {
            Netcode netcodeRef = gameInstanceRef.GetNetcode();
            if (netcodeRef.IsClient() && !netcodeRef.IsHost()) {
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
        currentMoney = 42;

        //To func?
        mainHUDRef.UpdatePlayerHealth(GetCurrentHealthPercentage(), currentPlayerID);
        mainHUDRef.UpdatePlayerMoneyCount(currentMoney, currentPlayerID);
        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.UpdatePlayerHealthServerRpc(currentHealth, Netcode.GetClientID());
        management.UpdatePlayerMoneyServerRpc(currentMoney, Netcode.GetClientID());


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
        bool jump = Input.GetKey (KeyCode.W);

        if (jump && isGrounded)
            Jump();

        if (left && right)
            inputDirection.x = 0.0f;
        else if (left)
            inputDirection.x = -1.0f;
        else if (right)
            inputDirection.x = 1.0f;
        else
            inputDirection.x = 0.0f;

        UpdateSpriteOrientation(inputDirection.x);


        Netcode netcodeRef = gameInstanceRef.GetNetcode();
        if (netcodeRef.IsHost()) {
            movingLeft = left;
            movingRight = right;
            if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
                SetMovementAnimationState(true);
            else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
                SetMovementAnimationState(false);
        }
        else if (netcodeRef.IsClient() && !netcodeRef.IsHost()) {

            if (inputDirection.x != 0.0f && !animatorRef.GetBool("isMoving"))
                gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(true);
            else if (inputDirection.x == 0.0f && animatorRef.GetBool("isMoving"))
                gameInstanceRef.GetRPCManagement().NotifyMovementAnimationStateServerRpc(false);
        }
    }
    private void Jump() {
        //For more consistent jump
        //It is not 100% accurate unit wise but is close enough
        //Raising gravity will effect it slighty.
        //For each increase in raisingGravity by 10.0f, reached height is increased by around 1 each time.

        rigidbody2DRef.gravityScale = raisingGravity; //Repeated here since it is required for the below calculation
        float jumpForce = Mathf.Sqrt(jumpMaximumHeight * -2 * (Physics2D.gravity.y * rigidbody2DRef.gravityScale));
        rigidbody2DRef.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
        //animatorComp.SetTrigger("isJumping");
    }


    //Animations
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
        velocity = inputDirection * currentSpeed;
        rigidbody2DRef.velocity = velocity;
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
        position.y += 0.1f;
        RaycastHit2D results 
            = Physics2D.BoxCast(position, boxCollider2DRef.size, 0.0f, Vector2.down, 0.2f, groundLayerMask.value);

        if (results) {
            isGrounded = true;

        }
        else {
            isGrounded = false;

        }
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


}
