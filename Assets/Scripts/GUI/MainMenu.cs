using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : Entity {


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }

    public void PlayButton() {
        gameInstanceRef.Transition(GameInstance.GameState.CONNECTION_MENU);
    }
    public void QuitButton() {
        gameInstanceRef.QuitApplication();
    }
}
