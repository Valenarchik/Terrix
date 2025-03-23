using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI errorMessage;

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
        BootstrapNetworkManager.Instance.CreateOrJoinDefaultLobby_OnClient();
    }
}