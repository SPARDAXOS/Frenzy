using Initialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static MyUtility.Utility;
using UnityEngine.UI;

public class MainHUD : Entity {


    [SerializeField] private Color LocalMessageColor = Color.blue;
    [SerializeField] private Color Player2MessageColor = Color.red;
    [SerializeField] private float chatMessageReceivedPopupDuration = 3.0f;
    [SerializeField] private float chatMessageSentPopupDuration = 2.0f;
    [SerializeField] private int chatMessagesRecycleLimit = 50;

    private bool chatOpened = false;
    private float chatMessagePopupTimer = 0.0f;


    private GameObject player1HUDElements;
    private GameObject player2HUDElements;

    private TMP_Text player1MoneyCountText;
    private TMP_Text player2MoneyCountText;

    private Image player1HealthBarImage;
    private Image player2HealthBarImage;

    private GameObject chat;
    private GameObject chatLogContent;
    private GameObject chatMessageReference;
    private TMP_InputField chatInputFieldComp;



    public override void Initialize(GameInstance game) {
        if (initialized)
            return;

        gameInstanceRef = game;
        SetupReferences();
        initialized = true;
    }
    public override void Tick() {
        if (!initialized)
            return;


        CheckInput();
        UpdateChatPopupTimer();
    }
    private void SetupReferences() {

        SetupPlayer1HUDReferences();
        SetupPlayer2HUDReferences();
        SetupChatHUDReferences();
    }

    private void SetupChatHUDReferences() {
        //Chat
        Transform chatTransform = transform.Find("Chat");
        Validate(chatTransform, "Failed to find chat reference", ValidationLevel.ERROR, true);
        chat = chatTransform.gameObject;

        //Chat Input Field
        Transform chatInputFieldTransform = chatTransform.Find("ChatInput").transform;
        Validate(chatInputFieldTransform, "Failed to find chat input field reference", ValidationLevel.ERROR, true);
        chatInputFieldComp = chatInputFieldTransform.GetComponent<TMP_InputField>();
        Validate(chatInputFieldComp, "Failed to find chat input field component reference", ValidationLevel.ERROR, true);

        //ChatLog Content
        Transform chatLogTransform = chatTransform.Find("ChatLog").transform;
        Validate(chatLogTransform, "Failed to find chat Log reference", ValidationLevel.ERROR, true);

        Transform chatLogViewportTransform = chatLogTransform.Find("LogViewport").transform;
        Validate(chatLogViewportTransform, "Failed to find Log Viewport reference", ValidationLevel.ERROR, true);

        Transform chatLogContentTransform = chatLogViewportTransform.Find("LogContent").transform;
        Validate(chatLogContentTransform, "Failed to find chat Log Content reference", ValidationLevel.ERROR, true);
        chatLogContent = chatLogContentTransform.gameObject;

        //ChatLog Message Reference
        Transform chatLogMessageReferenceTransform = chatLogTransform.Find("ChatMessageReference").transform;
        Validate(chatLogMessageReferenceTransform, "Failed to find Log Viewport reference", ValidationLevel.ERROR, true);
        chatMessageReference = chatLogMessageReferenceTransform.gameObject;
        chatMessageReference.SetActive(false);

        chatInputFieldComp.SetTextWithoutNotify("");
        SetChatState(false, false);
    }
    private void SetupPlayer1HUDReferences() {


        Transform player1HUDElementsTransform = transform.Find("Player1HUDElements");
        Validate(player1HUDElementsTransform, "Failed to find Player1HUDElements reference", ValidationLevel.ERROR, true);
        player1HUDElements = player1HUDElementsTransform.gameObject;

        //MoneyCount Text
        Transform player1MoneyIconTransform = player1HUDElementsTransform.Find("Player1MoneyIcon");
        Validate(player1MoneyIconTransform, "Failed to find Player1MoneyIcon reference", ValidationLevel.ERROR, true);

        Transform player1MoneyTextTransform = player1MoneyIconTransform.Find("Player1MoneyCount");
        Validate(player1MoneyTextTransform, "Failed to find Player1MoneyCount reference", ValidationLevel.ERROR, true);

        player1MoneyCountText = player1MoneyTextTransform.GetComponent<TMP_Text>();
        Validate(player1MoneyCountText, "Failed to find player1MoneyCountText reference", ValidationLevel.ERROR, true);


        //HealthBar Image
        Transform player1HealthBarFrameTransform = player1HUDElementsTransform.Find("Player1HealthBarFrame");
        Validate(player1HealthBarFrameTransform, "Failed to find player1HealthBarFrameTransform reference", ValidationLevel.ERROR, true);

        Transform player1HealthBarFillTransform = player1HealthBarFrameTransform.Find("Player1HealthBarFill");
        Validate(player1HealthBarFillTransform, "Failed to find player1HealthBarFillTransform reference", ValidationLevel.ERROR, true);

        player1HealthBarImage = player1HealthBarFillTransform.GetComponent<Image>();
        Validate(player1HealthBarImage, "Failed to find player1HealthBarImage reference", ValidationLevel.ERROR, true);
    }
    private void SetupPlayer2HUDReferences() {

        Transform player2HUDElementsTransform = transform.Find("Player2HUDElements");
        Validate(player2HUDElementsTransform, "Failed to find Player2HUDElements reference", ValidationLevel.ERROR, true);
        player2HUDElements = player2HUDElementsTransform.gameObject;


        //MoneyCount Text
        Transform player2MoneyIconTransform = player2HUDElementsTransform.Find("Player2MoneyIcon");
        Validate(player2MoneyIconTransform, "Failed to find Player2MoneyIcon reference", ValidationLevel.ERROR, true);

        Transform player2MoneyTextTransform = player2MoneyIconTransform.Find("Player2MoneyCount");
        Validate(player2MoneyTextTransform, "Failed to find Player2MoneyCount reference", ValidationLevel.ERROR, true);

        player2MoneyCountText = player2MoneyTextTransform.GetComponent<TMP_Text>();
        Validate(player2MoneyCountText, "Failed to find player2MoneyCountText reference", ValidationLevel.ERROR, true);

        //HealthBar Image
        Transform player2HealthBarFrameTransform = player2HUDElementsTransform.Find("Player2HealthBarFrame");
        Validate(player2HealthBarFrameTransform, "Failed to find player2HealthBarFrameTransform reference", ValidationLevel.ERROR, true);

        Transform player2HealthBarFillTransform = player2HealthBarFrameTransform.Find("Player2HealthBarFill");
        Validate(player2HealthBarFillTransform, "Failed to find player2HealthBarFillTransform reference", ValidationLevel.ERROR, true);

        player2HealthBarImage = player2HealthBarFillTransform.GetComponent<Image>();
        Validate(player2HealthBarImage, "Failed to find player2HealthBarImage reference", ValidationLevel.ERROR, true);
    }
    private void CheckInput() {
        if (Input.GetKeyDown(KeyCode.Return) && !chatOpened) {
            SetChatState(true, true, true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && chatOpened) {
            SetChatState(false, false);
        }
    }




    //Chat
    private void SetChatState(bool state, bool cursorState, bool select = false) {
        if (!chat)
            return;

        chatOpened = state;
        chat.SetActive(chatOpened);
        gameInstanceRef.SetCursorState(cursorState);
        if (chatOpened && select)
            chatInputFieldComp.Select();
    }
    public void AddReceivedChatMessage(string message) {
        if (string.IsNullOrEmpty(message))
            return;

        AddMessageToLog(message, false);
        if (!chatOpened)
            ActivateChatPopup(chatMessageReceivedPopupDuration);
    }
    private void AddMessageToLog(string message, bool local = true) {

        GameObject newMessage;
        int currentMessagesCount = chatLogContent.transform.childCount;
        if (currentMessagesCount == chatMessagesRecycleLimit) {
            newMessage = chatLogContent.transform.GetChild(0).gameObject; //Oldest message
            newMessage.transform.SetParent(null);
        }
        else
            newMessage = Instantiate(chatMessageReference);


        TMP_Text textComponent = newMessage.GetComponent<TMP_Text>();
        if (local) {
            textComponent.text = "Self: " + message;
            textComponent.color = LocalMessageColor;
        }
        else {
            textComponent.text = "Player 2: " + message;
            textComponent.color = Player2MessageColor;
        }

        newMessage.transform.SetParent(chatLogContent.transform);
        newMessage.SetActive(true);
    }
    private void ActivateChatPopup(float duration) {
        if (duration <= 0.0f)
            return;

        SetChatState(true, false);
        chatMessagePopupTimer = duration;
    }
    private void UpdateChatPopupTimer() {
        if (chatMessagePopupTimer <= 0.0f)
            return;

        chatMessagePopupTimer -= Time.deltaTime;
        if (chatMessagePopupTimer <= 0.0f) {
            chatMessagePopupTimer = 0.0f;
            SetChatState(false, false);
        }
    }
    public void ConfirmChatMessage() {
        string message = chatInputFieldComp.text;
        if (string.IsNullOrEmpty(message))
            return;

        RPCManagement management = gameInstanceRef.GetRPCManagement();
        management.SendChatMessageServerRpc(message, Netcode.GetClientID());
        AddMessageToLog(message);
        chatInputFieldComp.SetTextWithoutNotify("");
        ActivateChatPopup(chatMessageSentPopupDuration);
    }



    //PlayerHUDElements
    public void UpdatePlayerHealth(float amount, Player.PlayerID id) {
        if (id == Player.PlayerID.NONE)
            return;

        float value = amount;
        if (value < 0.0f)
            value *= -1;

        if (id == Player.PlayerID.PLAYER_1)
            player1HealthBarImage.fillAmount = value;
        else if (id == Player.PlayerID.PLAYER_2)
            player2HealthBarImage.fillAmount = value;
    }
    public void UpdatePlayerMoneyCount(int amount, Player.PlayerID id) {
        if (id == Player.PlayerID.NONE)
            return;

        int value = amount;
        if (value < 0.0f)
            value *= -1;

        if (id == Player.PlayerID.PLAYER_1)
            player1MoneyCountText.text = value.ToString();
        else if (id == Player.PlayerID.PLAYER_2)
            player2MoneyCountText.text = value.ToString();
    }



}
