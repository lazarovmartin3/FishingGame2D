using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Transform[] playerPositions;
    [SerializeField] private GameObject playerPreab;
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private GameObject onlinePlayerPrefab;
    [SerializeField] private MainMenuUI mainMenuUI;

    private int playerPositionIndex = 0;

    private List<GameObject> players = new List<GameObject>();
    private int gameID;
    public int GameID
    {
        get { return gameID; }
    }

    private void Awake()
    {
        instance = this;
    }

    public Transform GetPlayerPosition()
    {
        if(playerPositionIndex < playerPositions.Length) 
            return playerPositions[playerPositionIndex++];
        else
        {
            playerPositionIndex = 0;
            return playerPositions[playerPositionIndex++];
        }
    }

    public IEnumerator UpdateFromServer(WebSocketManager.ServerData data)
    {
        foreach (GameObject player in players)
        {
            if (player.GetComponent<Player>().GetID() == data.playerId)
            {
                yield return StartCoroutine(player.GetComponent<Player>().UpdatePlayerData(data));
            }
        }
    }

    public IEnumerator JoinGame(int gameID, int hostPlayerId , int playerID, int[] allPlayersID)
    {
        this.gameID = gameID;
        if(players.Count > 0)
        {
            for (int i = 0; i < allPlayersID.Length; i++)
            {
                if (playerID == allPlayersID[i])
                {
                    yield return StartCoroutine(CreatePlayer(allPlayersID[i], true));
                }
            }
        }
        else
        {
            for (int i = 0; i < allPlayersID.Length; i++)
            {
                if (playerID == allPlayersID[i])
                {
                    yield return StartCoroutine(CreatePlayer(allPlayersID[i], false));
                }
                else if(hostPlayerId == allPlayersID[i])
                {
                    yield return StartCoroutine(CreatePlayer(allPlayersID[i], true));
                }

            }
        }
        
        mainMenuUI.GameCreated();
    }

    public IEnumerator CreateNewOnlineGame(int gameID, int playerID)
    {
        this.gameID = gameID;
        yield return StartCoroutine(CreatePlayer(playerID, false));
        mainMenuUI.GameCreated();
    }

    private IEnumerator CreatePlayer(int playerID, bool disabledInput)
    {
        GameObject player = Instantiate(onlinePlayerPrefab, playerPositions[playerPositionIndex++]);
        player.GetComponent<Player>().SetID(playerID);
        player.GetComponent<Player>().DisabledInput = disabledInput;
        players.Add(player);

        // Wait until the player setup is completed
        yield return new WaitForEndOfFrame();

        // Indicate that player creation is complete
        yield return true;
    }
}