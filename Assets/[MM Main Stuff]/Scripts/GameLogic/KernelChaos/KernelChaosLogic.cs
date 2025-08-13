using System;
using System.Collections.Generic;
using UnityEngine;

public class KernelChaosLogic : IGameLogicHandler
{
    private readonly GameObject _playerPrefab;
    private readonly List<Transform> _spawnPoints = new();
    private int _nextSpawnIndex = 0;

    // Constructor that initializes the prefab and spawn points
    public KernelChaosLogic(GameObject playerPrefab)
    {
        _playerPrefab = playerPrefab;
        CacheSpawnPoints();
    }

    // Cache spawn points from the scene
    private void CacheSpawnPoints()
    {
        _spawnPoints.Clear();

        var parent = GameObject.Find("KC_SpawnPoints");
        if (parent != null)
        {
            foreach (Transform t in parent.transform)
            {
                if (t != null) _spawnPoints.Add(t);
            }
        }

        if (_spawnPoints.Count == 0)
        {
            Debug.LogWarning("🌽 [KC] No spawn points found.");
        }
    }

    // Spawn a player at a random spawn point
    public GameObject SpawnPlayer(string username, string team = null)
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("🌽 [KC] Missing player prefab.");
            return null;
        }

        Transform sp = _spawnPoints[_nextSpawnIndex++ % _spawnPoints.Count];
        var pos = sp.position;
        var rot = sp.rotation;

        var go = UnityEngine.Object.Instantiate(_playerPrefab, pos, rot);
        var playerScript = go.GetComponent<PlayerKernelChaos>();
        if (playerScript != null)
        {
            playerScript.Initialize(username);  // Initialize the player's avatar
        }

        Debug.Log($"🌽 [KC] Spawned '{username}' at {sp.name}");
        return go;
    }

    // Handle actions from players, such as movement, jump, etc.
    public void HandleAction(GameObject playerObject, string action)
    {
        if (playerObject == null || string.IsNullOrEmpty(action)) return;

        // For now, log the action
        Debug.Log($"🌽 [KC] Player {playerObject.name} performing action: {action}");

        // Add custom action handling here (movement, jump, etc.)
        if (action == "move")
        {
            // Handle movement (e.g., using velocity or position)
        }
        else if (action == "jump")
        {
            // Handle jump action
        }
    }

    // Called when the game starts
    public void OnStartGame()
    {
        // Game start logic here
        Debug.Log("🌽 [KC] Game Started");
    }

    // Called when the game ends
    public void OnEndGame()
    {
        // Game end logic here
        Debug.Log("🌽 [KC] Game Ended");
    }

    // Handle player joining the game
    public void OnPlayerJoin(string username, string team = null)
    {
        // Call the spawn player method when a player joins
        Debug.Log($"🌽 [KC] Player {username} joining the game.");
        SpawnPlayer(username, team);
    }
}
