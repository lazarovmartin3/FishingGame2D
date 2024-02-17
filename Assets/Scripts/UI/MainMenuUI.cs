using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button multiplayerBtn, quitBtn;
    [SerializeField] private GameObject networkMenu, mainMenu;
    [SerializeField] private Button createMultiplayerGameBtn, joinGameBtn, backToMainBtn;
    [SerializeField] private GameObject onlineGamesPrefab;
    [SerializeField] private Transform allGamesParent;
    [SerializeField] private TextMeshProUGUI connectionStatusTxt;

    private void Start()
    {
        multiplayerBtn.onClick.AddListener(OpenNetworkMenu);

        createMultiplayerGameBtn.onClick.AddListener(CreateMultiplayerGame);
        backToMainBtn.onClick.AddListener(BackToMainMenu);
        joinGameBtn.onClick.AddListener(ClientJoin);
        quitBtn.onClick.AddListener(() => Application.Quit());
    }

    private void ClientJoin()
    {
        BackToMainMenu();
        NetworkManager.Singleton.StartClient();
        this.gameObject.SetActive(false);
    }

    private void OpenNetworkMenu()
    {
        networkMenu.SetActive(true);
    }

    private void BackToMainMenu()
    {
        networkMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void CreateMultiplayerGame()
    {
        NetworkManager.Singleton.StartHost();
        BackToMainMenu();
        this.gameObject.SetActive(false);
        FishSpawner.instance.SpawnFish(20);
    }
}