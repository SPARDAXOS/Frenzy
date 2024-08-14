using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static GameInstance;
using static MyUtility.Utility;


public class RPCManagement : NetworkedEntity {


    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;




    }
    private ClientRpcParams? CreateClientRpcParams(ulong senderID) {
        Netcode netcodeRef = gameInstanceRef.GetNetcode();
        var targetID = netcodeRef.GetOtherClient(senderID); //Do more elegant solution
        if (targetID == senderID) {
            Log("Other client look up failed!");
            return null;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams();
        clientRpcParams.Send = new ClientRpcSendParams();
        clientRpcParams.Send.TargetClientIds = new ulong[] { targetID };
        return clientRpcParams;
    }



    [ServerRpc (RequireOwnership = true)]
    public void ConfirmConnectionServerRpc() {
        RelayConnectionConfirmationClientRpc();
    }
    [ClientRpc]
    public void RelayConnectionConfirmationClientRpc() {
        gameInstanceRef.StartGame();
    }


    //References
    [ClientRpc]
    public void RelayPlayerReferenceClientRpc(NetworkObjectReference reference, Player.PlayerID player, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.SetReceivedPlayerReferenceRpc(reference, player);
    }


    //Input
    [ServerRpc(RequireOwnership = false)]
    public void CalculatePlayer2PositionServerRpc(float input) {
        gameInstanceRef.ProccessPlayer2MovementRpc(input);
    }

    //Animations
    [ServerRpc(RequireOwnership = false)]
    public void NotifyMovementAnimationStateServerRpc(bool state) {
        gameInstanceRef.ProcessPlayer2MovementAnimationState(state);
    }


    //Sprite Orientation
    [ServerRpc(RequireOwnership = false)]
    public void SendSpriteOrientationServerRpc(bool flipX, ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelaySpriteOrientationClientRpc(flipX, (ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelaySpriteOrientationClientRpc(bool flipX, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.ProcessPlayerSpriteOrientation(flipX);
    }



    //Chat
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(FixedString32Bytes message, ulong senderID) {
        ClientRpcParams? clientRpcParams = CreateClientRpcParams(senderID);
        if (clientRpcParams == null)
            return;

        RelayChatMessageClientRpc(message, (ClientRpcParams)clientRpcParams);
    }
    [ClientRpc]
    public void RelayChatMessageClientRpc(FixedString32Bytes message, ClientRpcParams clientRpcParameters = default) {
        gameInstanceRef.ProccessReceivedChatMessage(message.ToString());
    }
}
