using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class WebSocketManager : MonoBehaviour
{
    public static WebSocketManager instance;

    private WebSocket ws;
    private WebSocketMessageHandler messageHandler;
    private bool receivedResponse;
    public string responseData;

    public static event Action OnGamesUpdated;

    private List<int> AllGames = new List<int>();
    private Action<string> OnConnectionError;
    private List<string> messagesQueue = new List<string>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //messageHandler = GetComponent<WebSocketMessageHandler>();
        StartCoroutine(HandleMessageRespose());
    }

    public void ConnectToLocalServer(Action OnConnected, Action<string> OnError)
    {
        this.OnConnectionError = OnError;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ws = new WebSocket("ws://localhost:8080");
        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to server");
            OnConnected.Invoke();
            GetAllGames();
        };

        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Received message: " + e.Data);
            // Handle received message
            responseData = e.Data;
            receivedResponse = true;
            messagesQueue.Add(responseData);
            //OnMessageReceived?.Invoke();
            //HandleMessage();
            //messageHandler.ReceiveMessage(e.Data);
        };
        ws.OnError += (sender, e) =>
        {
            Debug.LogError("WebSocket error: " + e.Message);
            this.OnConnectionError.Invoke(e.Message);
        };
        ws.OnClose += (sender, e) =>
        {
            Debug.Log("Disconnected from server");
        };

        ws.Connect();
#endif
    }

    [DllImport("__Internal")]
    private static extern void ConnectToLocalServer();
    
    [DllImport("__Internal")]
    public static extern void SendToServer(string message);

    public void ConnectWEBGL()
    {
        ConnectToLocalServer();
    }
    
//    public void ConnectToLocalServer()
//    {
//#if UNITY_WEBGL && !UNITY_EDITOR
//        Application.ExternalCall("WebSocketInt.ConnectToLocalServer");
//#endif
//    }

    public void OnConnected()
    {
        Debug.Log("Connected to server");
    }

    public void OnMessage(string message)
    {
        Debug.Log("Received message from web: " + message);
        // Handle received message
        responseData = message;
        receivedResponse = true;
        messagesQueue.Add(responseData);
    }

    public void OnError(string errorMessage)
    {
        Debug.LogError("WebSocket error: " + errorMessage);
        // Handle WebSocket error
    }


    public void CreateGame()
    {
        ServerData data = new ServerData();
        data.action = "CreateGame";
        string json = JsonUtility.ToJson(data);
        SendMessageToServer(json);
    }

    public void JoinGame(int gameId)
    {
        ServerData data = new ServerData();
        data.action = "JoinGame";
        data.gameId = gameId;
        string json = JsonUtility.ToJson(data);
        SendMessageToServer(json);
    }

    public void GetAllGames()
    {
        ServerData data = new ServerData();
        data.action = "GetAllGameIds";
        string json = JsonUtility.ToJson(data);
        SendMessageToServer(json);
    }

    public void CatchFish(int id)
    {
        FishUpdate data = new FishUpdate();
        data.action = "CatchFish";
        data.gameId = GameManager.instance.GameID;
        data.fishId = id;
        string json = JsonUtility.ToJson(data);
        SendMessageToServer(json);
    }

    public void SendMessageToServer(string message)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ws.Send(message);
#elif UNITY_WEBGL
        SendToServer(message);
#endif
    }

    /*public IEnumerator SendMessageToServer(string message)
    {
        ws.Send(message);
        /*yield return new WaitUntil(() => receivedResponse);
        if (receivedResponse)
        {
            ServerData jsonData = JsonUtility.FromJson<ServerData>(responseData);
            print("Data action " + jsonData.action);
            receivedResponse = false;
            if (jsonData.action == "UpdateHookPosition")
            {
                print("handle UpdateHookPosition");
                yield return StartCoroutine(GameManager.instance.UpdateFromServer(jsonData));
            }
            if (jsonData.action == "AllGameIds")
            {
                AllGames.Clear();
                print("All games " +  jsonData.gameIds.Length);
                for (int i = 0; i < jsonData.gameIds.Length; i++)
                {
                    AllGames.Add(jsonData.gameIds[i]);
                }
                OnGamesUpdated?.Invoke();
            }
            if (jsonData.action == "JoinedGame")
            {
                print("Handle Join game " + jsonData.gameId);
                yield return StartCoroutine(GameManager.instance.JoinGame(jsonData.gameId, jsonData.playerId, jsonData.playerIds));
            }
            if (jsonData.action == "GameNotFound")
            {
                OnError.Invoke("Game not Found!!!");
            }
        }
        else
        {
            Debug.LogError("No response received from the server.");
        }
        yield return null;
    }*/

    private IEnumerator HandleMessageRespose()
    {
        yield return new WaitUntil(() => messagesQueue.Count > 0);
        if (receivedResponse)
        {
            ServerData jsonData = JsonUtility.FromJson<ServerData>(responseData);
            print("Data action " + jsonData.action);
            receivedResponse = false;
            if (jsonData.action == "UpdateHookPosition")
            {
                print("handle UpdateHookPosition");
                yield return StartCoroutine(GameManager.instance.UpdateFromServer(jsonData));
            }
            if (jsonData.action == "AllGameIds")
            {
                AllGames.Clear();
                print("All games " + jsonData.gameIds.Length);
                for (int i = 0; i < jsonData.gameIds.Length; i++)
                {
                    AllGames.Add(jsonData.gameIds[i]);
                }
                OnGamesUpdated?.Invoke();
            }
            if (jsonData.action == "CreatedGame")
            {
                yield return StartCoroutine(GameManager.instance.CreateNewOnlineGame(jsonData.gameId, jsonData.playerId));
                FishUpdateData fishData = JsonUtility.FromJson<FishUpdateData>(responseData);
                FishSpawner.instance.SpawnOnlineFish(fishData.fishIds);
            }
            if (jsonData.action == "JoinedGame")
            {
                yield return StartCoroutine(GameManager.instance.JoinGame(jsonData.gameId, jsonData.hostPlayerId, jsonData.playerId, jsonData.playerIds));
                FishUpdateData fishData = JsonUtility.FromJson< FishUpdateData>(responseData);
                FishSpawner.instance.SpawnOnlineFish(fishData.fishIds);
            }
            if (jsonData.action == "FishUpdate")
            {
                FishUpdateData fishData = JsonUtility.FromJson<FishUpdateData>(responseData);
                FishSpawner.instance.UpdateFishPosition(fishData.fishIds);
            }
            if (jsonData.action == "GameNotFound")
            {
                Debug.Log("Game not found ");
                //OnConnectionError.Invoke("Game not Found!!!");
            }
        }
        messagesQueue.Clear();
        StartCoroutine(HandleMessageRespose());
    }

    public List<int> GetAllGamesIDs() { return AllGames; }

    public void Disconnect()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }

    void OnDestroy()
    {
        Disconnect();
    }

    [Serializable]
    public class ServerData
    {
        public string action;
        public int playerId;
        public int[] playerIds;
        public int hostPlayerId;
        public int gameId;
        public int[] gameIds;
        public Vector3 targetPosition;
        public Vector3 hookInitPosition;
        public bool isCasting, isRetracting, isThrowing;
    }

    [Serializable]
    public class FishUpdate
    {
        public string action;
        public int gameId;
        public Vector2 position;
        public int[] fishIds;
        public int fishId;
    }

    [Serializable]
    public class FishData
    {
        public int id;
        public float x;
        public float y;
    }

    [Serializable]
    public class FishUpdateData
    {
        public string action;
        public FishData[] fishIds;
    }
}