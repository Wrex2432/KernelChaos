// [KC] Kernel Chaos - constructor-based game logic matching IGameLogicHandler
using System;
using System.Collections.Generic;
using UnityEngine;

public class KernelChaosLogic : IGameLogicHandler {
    private readonly GameObject _playerPrefab;
    private readonly List<Transform> _spawnPoints = new();
    private int _nextSpawnIndex = 0;

    // Matches GameLogicRegistry: new KernelChaosLogic(kernelChaosPrefab)
    public KernelChaosLogic (GameObject playerPrefab) {
        _playerPrefab = playerPrefab;
        CacheSpawnPoints();
    }

    private void CacheSpawnPoints () {
        _spawnPoints.Clear();

        // Preferred: parent named "KC_SpawnPoints" with child points
        var parent = GameObject.Find("KC_SpawnPoints");
        if (parent != null) {
            foreach (Transform t in parent.transform) {
                if (t != null) _spawnPoints.Add(t);
            }
        }

        // Fallback: any transforms named like "KC_SpawnPoint_*"
        if (_spawnPoints.Count == 0) {
            var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var t in all) {
                if (t == null) continue;
                if (t.name.StartsWith("KC_SpawnPoint", StringComparison.OrdinalIgnoreCase)) {
                    _spawnPoints.Add(t);
                }
            }
        }

        if (_spawnPoints.Count == 0) {
            Debug.LogWarning("🌽 [KC] No spawn points found. Create 'KC_SpawnPoints' with children, or name children 'KC_SpawnPoint_01' etc.");
        }
    }

    public GameObject SpawnPlayer (string username, string team = null) {
        if (_playerPrefab == null) {
            Debug.LogError("🌽 [KC] Missing player prefab.");
            return null;
        }

        Transform sp = null;
        if (_spawnPoints.Count > 0) {
            if (_nextSpawnIndex >= _spawnPoints.Count) _nextSpawnIndex = 0;
            sp = _spawnPoints[_nextSpawnIndex++];
        }

        var pos = sp ? sp.position : Vector3.zero;
        var rot = sp ? sp.rotation : Quaternion.identity;

        var go = UnityEngine.Object.Instantiate(_playerPrefab, pos, rot);
        var p = go.GetComponent<PlayerKernelChaos>();
        if (p != null) p.Initialize(string.IsNullOrWhiteSpace(username) ? "Player" : username);

        Debug.Log($"🌽 [KC] Spawned '{username}' at {(sp ? sp.name : "origin")}");
        return go;
    }

    public void HandleAction (GameObject playerObject, string action) {
        // Reserved for web actions later; noop for now
        if (playerObject == null || string.IsNullOrEmpty(action)) return;
        Debug.Log($"🌽 [KC] HandleAction: {playerObject.name} -> {action}");
    }

    public void OnPlayerJoin (string username, string team = null) {
        SpawnPlayer(username, team);
    }

    public void OnStartGame () {
        Debug.Log("🌽 [KC] OnStartGame");
    }

    public void OnEndGame () {
        Debug.Log("🌽 [KC] OnEndGame");
    }
}
