using System;
using FishNet.Object;
using Terrix.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI errorMessage;
    [SerializeField] private Image colorPreviewImage;
    [SerializeField] private TMP_InputField playerInput;
    // [SerializeField] private PlayerDataHolder playerDataHolder;

    public void CreateCustomLobby()
    {
        BootstrapNetworkManager.Instance.CreateCustomLobby_OnClient();
    }

    public void JoinCustomLobby()
    {
        BootstrapNetworkManager.Instance.TryJoinCustomLobby(int.Parse(lobbyInput.text));
    }

    public void StartDefaultGame()
    {
        // throw new Exception("Button was clicked on client");
        Debug_OnServer();
        PlayerDataHolder.SetData(colorPreviewImage.color, playerInput.text);
        BootstrapNetworkManager.Instance.CreateOrJoinDefaultLobby_OnClient();
        // throw new Exception("Button was clicked on client");
    }

    [ServerRpc(RequireOwnership = false)]
    private void Debug_OnServer()
    {
        Debug.Log("Button was clicked on server");
    }
}