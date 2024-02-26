using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketMessageHandler : MonoBehaviour
{
    public static event Action OnGameCreated;
    public static ServerMessage serverData;

    public void ReceiveMessage(string message)
    {
        ServerMessage data = JsonUtility.FromJson<ServerMessage>(message);

        print(data.action);

        if (data.action == "UpdateHookPosition")
        {
            //GameManager.instance.UpdateFromServer(data);
        }
        if (data.action == "CreatedGame")
        {
            print("Creating new game");
            serverData = data;
            //OnGameCreated.Invoke();
            //GameManager.instance.Test(data.gameId, data.playerId);
            //StartCoroutine(GameManager.instance.CreateNewOnlineGame(data.gameId, data.playerId));
        }
    }

    [System.Serializable]
    public class ServerMessage
    {
        public string action;
        public int playerId;
        public int gameId;
        public Vector3 targetPosition;
        public bool isCasting, isRetracting, isThrowing;
    }
}