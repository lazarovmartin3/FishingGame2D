using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlineGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameIDTxt;

    private int gameID;

    public int GameID
    {
        set { gameID = value; }
        get { return gameID; }
    }

    public void EditTitle(int gameId)
    {
        gameIDTxt.text = gameId.ToString();
        this.gameID = gameId;
    }
}
