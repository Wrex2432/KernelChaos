using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab; // Player prefab to spawn
    public Transform[] spawnPoints; // Array of spawn points to pick from

    void Update()
    {
        // Check if the "M" key is pressed
        if (Input.GetKeyDown(KeyCode.M))
        {
            // For now, we're using a placeholder name "Player" for the spawned player
            SpawnPlayer("Player" + Random.Range(1, 1000)); // Generate a random name for each player
        }
    }

    // Method to spawn the player
    public void SpawnPlayer(string username)
    {
        if (playerPrefab != null && spawnPoints.Length > 0)
        {
            // Get a random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate the player at the spawn point
            GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            playerObj.name = username;

            // Log the player spawn for debugging
            Debug.Log($"Player {username} spawned at {spawnPoint.position}");
        }
        else
        {
            Debug.LogError("Player prefab or spawn points are not set!");
        }
    }

    public void SpawnPlayerFromWebSocket(string username)
    {
        if (playerPrefab != null && spawnPoints.Length > 0)
        {
            // Get a random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Instantiate the player at the spawn point
            GameObject playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            playerObj.name = username;

            // Log the player spawn for debugging
            Debug.Log($"Player {username} spawned from WebSocket at {spawnPoint.position}");
        }
        else
        {
            Debug.LogError("Player prefab or spawn points are not set!");
        }
    }
}
