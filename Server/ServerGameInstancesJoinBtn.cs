using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerGameInstancesJoinBtn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameIDTxt;
    public int gameID;

    private void Start()
    {
        gameIDTxt.text = gameID.ToString();
        GetComponent<Button>().onClick.AddListener(() => ServerManager.instance.JoinGame(gameID));
    }
}
