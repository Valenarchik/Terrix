using CustomUtilities.DataStructures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject menuScreen, lobbyScreen;
    [SerializeField] private TMP_InputField lobbyInput;
    [SerializeField] private TextMeshProUGUI lobbyTitle, lobbyIDText;
    [SerializeField] private Button startGameButton;

    // private void Start()
    // {
    //     OpenMainMenu();
    // }

    public void CreateLobby()
    {
        // BootstrapManager.CreateLobby();
        StartNewGame();
    }

    public void StartGame()
    {
        // BootstrapManager.CreateLobby();
        StartNewGame();
    }

    public void OpenMainMenu()
    {
        CloseAllScreens();
        menuScreen.SetActive(true);
    }
    
    void CloseAllScreens()
    {
        menuScreen.SetActive(false);
        lobbyScreen.SetActive(false);
    }
    

    public void StartNewGame()
    {
        BootstrapNetworkManager.Instance.CreateOrJoinLobby_OnClient();
    }
}