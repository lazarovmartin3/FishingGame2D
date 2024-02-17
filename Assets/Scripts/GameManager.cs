using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Transform[] playerPositions;
    [SerializeField] private GameObject playerPreab;
    [SerializeField] private FishSpawner fishSpawner;

    private int playerPositionIndex = 0;

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
}