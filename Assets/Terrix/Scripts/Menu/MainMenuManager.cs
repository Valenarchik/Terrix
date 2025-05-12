using System;
using FishNet.Object;
using Terrix.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : NetworkBehaviour
{
    // [SerializeField] private TMP_InputField lobbyInput;
    // [SerializeField] private TextMeshProUGUI errorMessage;
    [SerializeField] private Image colorPreviewImage;
    [SerializeField] private TMP_InputField playerInput;
    [SerializeField] private CustomLobbySettingsUI customLobbySettingsUI;
    [SerializeField] private EnterCustomLobbyUI enterCustomLobbyUI;
    [SerializeField] private TMP_InputField botsCountInputField;
    [SerializeField] private TMP_InputField lobbiesCountInputField;
    [SerializeField] private TextMeshProUGUI customLobbyErrorText;

    public static MainMenuManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void CreateCustomLobby()
    {
        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        LobbyManager.Instance.CreateCustomLobby_OnClient(customLobbySettingsUI.GetLobbySettings());
    }

    public void JoinCustomLobby()
    {
        var input = enterCustomLobbyUI.GetInput();
        if (!int.TryParse(input, out var id) || id is < 100000 or > 999999)
        {
            ShowCustomLobbyErrorText("Используйте 6-значное число");
            return;
        }

        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        LobbyManager.Instance.TryJoinCustomLobby_OnClient(id);
    }

    public void StartDefaultGame()
    {
        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        LobbyManager.Instance.CreateOrJoinDefaultLobby_OnClient();
        // BootstrapNetworkManager.Instance.CreateOrJoinDefaultLobby_OnClient();
    }

    public void ShowCustomLobbyErrorText(string text)
    {
        customLobbyErrorText.text = text;
        customLobbyErrorText.gameObject.SetActive(true);
    }

    public void StartFakeGame()
    {
        var botsCount = Convert.ToInt32(botsCountInputField.text);
        var lobbiesCount = Convert.ToInt32(lobbiesCountInputField.text);
        for (int i = 0; i < lobbiesCount; i++)
        {
            LobbyManager.Instance.CreateFakeLobby_OnClient(botsCount);
        }
        // BootstrapNetworkManager.Instance.CreateFakeLobby_OnClient();
    }
}