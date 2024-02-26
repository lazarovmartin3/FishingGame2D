using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FishSpawner : MonoBehaviour
{
    public static FishSpawner instance;

    [SerializeField] private Tilemap tilemap; // Reference to the Tilemap component
    [SerializeField] private GameObject[] fishPrefabs;
    private List<Fish> fishList;

    private void Awake()
    {
        instance = this;
    }

    /*public void SpawnFish(int amount)
    {
        if (IsServer)
        {
            BoundsInt bounds = tilemap.cellBounds; // Get the bounds of the tilemap
            Vector3Int minCell = bounds.min; // Get the minimum cell coordinate
            Vector3Int maxCell = bounds.max; // Get the maximum cell coordinate

            for (int i = 0; i < amount; i++)
            {
                Vector3Int randomCell = new Vector3Int(Random.Range(minCell.x, maxCell.x), Random.Range(minCell.y, maxCell.y), minCell.z);
                // Generate a random cell within the bounds of the tilemap

                if (tilemap.HasTile(randomCell)) // Check if the random cell has a tile
                {
                    Vector3 spawnPosition = tilemap.GetCellCenterWorld(randomCell); // Get the world position of the cell center
                    int randomFish = Random.Range(0, fishPrefabs.Length);
                    NetworkObject fish = Instantiate(fishPrefabs[randomFish], spawnPosition, Quaternion.identity).GetComponent<NetworkObject>();
                    fish.Spawn();
                    fish.transform.SetParent(transform);
                }
                else
                {
                    i--; // If the random cell doesn't have a tile, decrement the loop counter to try again
                }
            }
        }
    }*/

    public void SpawnOnlineFish(WebSocketManager.FishData[] fishIDs)
    {
        BoundsInt bounds = tilemap.cellBounds; // Get the bounds of the tilemap
        Vector3Int minCell = bounds.min; // Get the minimum cell coordinate
        Vector3Int maxCell = bounds.max; // Get the maximum cell coordinate
        fishList = new List<Fish>();

        for (int i = 0; i < fishIDs.Length; i++)
        {
            Vector3Int randomCell = new Vector3Int(Random.Range(minCell.x, maxCell.x), Random.Range(minCell.y, maxCell.y), minCell.z);
            // Generate a random cell within the bounds of the tilemap

            if (tilemap.HasTile(randomCell)) // Check if the random cell has a tile
            {
                Vector3 spawnPosition = tilemap.GetCellCenterWorld(randomCell); // Get the world position of the cell center
                int randomFish = Random.Range(0, fishPrefabs.Length);
                GameObject fish = Instantiate(fishPrefabs[randomFish], spawnPosition, Quaternion.identity);
                fish.GetComponent<Fish>().id = fishIDs[i].id;
                fish.transform.SetParent(transform);
                fishList.Add(fish.GetComponent<Fish>());
            }
            else
            {
                i--; // If the random cell doesn't have a tile, decrement the loop counter to try again
            }
        }
    }

    public void UpdateFishPosition(WebSocketManager.FishData[] fishIDs)
    {
        if (fishIDs == null) print("Fish ids null");
        else print("Fish " + fishIDs.Length);
        for(int i = 0;i < fishIDs.Length; i++)
        {
            if (fishList[i].id == fishIDs[i].id)
            {
                fishList[i].UpdatePosition(new Vector2(fishIDs[i].x, fishIDs[i].y));
            }
        }
    }
}