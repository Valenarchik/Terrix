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

    public void CreateCustomLobby()
    {
        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        BootstrapNetworkManager.Instance.CreateCustomLobby_OnClient(customLobbySettingsUI.GetLobbySettings());
    }

    public void JoinCustomLobby()
    {
        var inputID = enterCustomLobbyUI.GetInputID();
        if (inputID == 0)
        {
            return;
        }

        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        BootstrapNetworkManager.Instance.TryJoinCustomLobby(inputID);
    }

    public void StartDefaultGame()
    {
        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        BootstrapNetworkManager.Instance.CreateOrJoinDefaultLobby_OnClient();
    }

    public void StartFakeGame()
    {
        var botsCount = Convert.ToInt32(botsCountInputField.text);
        var lobbiesCount = Convert.ToInt32(lobbiesCountInputField.text);
        for (int i = 0; i < lobbiesCount; i++)
        {
            BootstrapNetworkManager.Instance.CreateFakeLobby_OnClient(botsCount);
        }
        // BootstrapNetworkManager.Instance.CreateFakeLobby_OnClient();
    }
}