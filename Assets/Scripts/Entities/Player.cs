using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity {

    public enum PlayerID {
        NONE = 0,
        PLAYER_1,
        PLAYER_2
    }


    private PlayerID currentPlayerID = PlayerID.NONE;

    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {

    }
    public override void FixedTick() {


    }

    public PlayerID GetPlayerID() { return currentPlayerID; }
    public void SetPlayerID(PlayerID id) { currentPlayerID = id; }


}
