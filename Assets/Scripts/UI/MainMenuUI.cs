using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button multiplayerBtn, quitBtn;
    [SerializeField] private GameObject networkMenu, mainMenu;
    [SerializeField] private Button createMultiplayerGameBtn, joinGameBtn, backToMainBtn;
    [SerializeField] private GameObject onlineGamesPrefab;
    [SerializeField] private Transform allGamesParent;
    [SerializeField] private TextMeshProUGUI connectionStatusTxt;
    [SerializeField] private Button refreshBtn;

    private void Start()
    {
        multiplayerBtn.onClick.AddListener(OpenNetworkMenu);

        createMultiplayerGameBtn.onClick.AddListener(CreateMultiplayerGame);
        backToMainBtn.onClick.AddListener(BackToMainMenu);
        joinGameBtn.onClick.AddListener(ClientJoin);
        quitBtn.onClick.AddListener(() => Application.Quit());
        refreshBtn.onClick.AddListener(() => WebSocketManager.instance.GetAllGames());
    }

    private void OnEnable()
    {
        WebSocketManager.OnGamesUpdated += ShowAllGames;
    }

    private void ClientJoin()
    {
        //BackToMainMenu();
        //NetworkManager.Singleton.StartClient();
        //this.gameObject.SetActive(false);
    }

    private void OpenNetworkMenu()
    {
        networkMenu.SetActive(true);
        Debug.Log("Connecting to server...");
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        WebSocketManager.instance.ConnectToLocalServer(OnServerConnected, OnServerError);
#elif UNITY_WEBGL
        WebSocketManager.instance.ConnectWEBGL();
#endif
    }

    private void BackToMainMenu()
    {
        networkMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void CreateMultiplayerGame()
    {
        /* OLD NETWORK MANAGER
        NetworkManager.Singleton.StartHost();
        BackToMainMenu();
        this.gameObject.SetActive(false);
        FishSpawner.instance.SpawnFish(20);*/

        WebSocketManager.instance.CreateGame();
        connectionStatusTxt.text = "Connecting to server...";
    }

    private void OnServerConnected()
    {
    }

    private void OnServerError(string error)
    {
        connectionStatusTxt.text = error;
    }

    private void ShowAllGames()
    {
        for (int i = 0; i < allGamesParent.childCount; i++)
        {
            Destroy(allGamesParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < WebSocketManager.instance.GetAllGamesIDs().Count; i++)
        {
            GameObject game = Instantiate(onlineGamesPrefab, allGamesParent);
            int gameId = WebSocketManager.instance.GetAllGamesIDs()[i];
            game.GetComponent<OnlineGameUI>().EditTitle(gameId);
            game.GetComponent<Button>().onClick.AddListener(()=> JoinGame(gameId));
        }
    }

    private void JoinGame(int gameID)
    {
        print("Joingin game " + gameID);
        joinGameBtn.GetComponentInChildren<TextMeshProUGUI>().text = "JoinGame " + gameID;
        joinGameBtn.onClick.AddListener(() => WebSocketManager.instance.JoinGame(gameID));
    }

    public void GameCreated()
    {
        connectionStatusTxt.text = "";
        this.gameObject.SetActive(false);
    }
}