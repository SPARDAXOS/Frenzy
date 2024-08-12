using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using static MyUtility.Utility;

public class Player : Entity {

    [SerializeField] private float accelerationSpeed = 1.0f;
    [SerializeField] private float decelerationSpeed = 1.0f;
    [SerializeField] private float maxSpeed = 100.0f;




    public enum PlayerID {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }

    private PlayerID currentPlayerID = PlayerID.NONE;


    private SpriteRenderer spriteRendererRef;
    private Animator animatorRef;
    private Rigidbody2D rigidbody2DRef;

    public float currentSpeed = 0.0f;
    public Vector2 direction = Vector2.zero;
    public Vector2 velocity = Vector2.zero;

    private bool movingRight = false;
    private bool movingLeft = false;


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;


        spriteRendererRef = GetComponent<SpriteRenderer>();
        animatorRef = GetComponent<Animator>();
        rigidbody2DRef = GetComponent<Rigidbody2D>();

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;
        if (gameInstanceRef.GetNetcode().IsClient() && currentPlayerID == PlayerID.PLAYER_1)
            return;


        movingLeft = Input.GetKey(KeyCode.A);
        movingRight = Input.GetKey(KeyCode.D);

        if (movingLeft && movingRight)
            direction.x = 0.0f;
        else if (movingLeft)
            direction.x = -1.0f;
        else if (movingRight)
            direction.x = 1.0f;
    }
    public override void FixedTick() {
        if (!initialized)
            return;
        if (currentPlayerID == PlayerID.NONE)
            return;
        if (gameInstanceRef.GetNetcode().IsClient() && currentPlayerID == PlayerID.PLAYER_1)
            return;

        if (movingRight || movingLeft)
            Accelerate();
        else
            Decelerate();

        UpdateMovement();
    }
    public void SetupStartingState() {
        if (!initialized)
            return;


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
    private void UpdateMovement() {
        velocity = direction * currentSpeed;
        rigidbody2DRef.velocity = velocity;
    }



    public PlayerID GetPlayerID() { return currentPlayerID; }
    public void SetPlayerID(PlayerID id) { currentPlayerID = id; }


}
