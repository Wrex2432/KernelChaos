using UnityEngine;
using System.Collections.Generic;

public class PlayerTracker : MonoBehaviour {
    public static PlayerTracker Instance;

    private Dictionary<string, GameObject> playerObjects = new();
    public bool gameStarted = false;

    private string localPlayerUsername; // ✅ Local player identity

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(string username, string team = null) {
        if (playerObjects.ContainsKey(username)) {
            Debug.LogWarning($"⚠️ Player {username} already registered.");
            return;
        }

        GameObject obj = Initializer.ActiveGameLogic?.SpawnPlayer(username, team);
        if (obj != null) {
            playerObjects[username] = obj;
            Initializer.ActiveGameLogic?.OnPlayerJoin(username, team);
            GameSessionSaver.Instance?.RegisterPlayer(username);
        }
    }

    public void HandleAction(string username, string action) {
        if (playerObjects.TryGetValue(username, out GameObject obj)) {
            Initializer.ActiveGameLogic?.HandleAction(obj, action);
        } else {
            Debug.LogWarning($"⚠️ No GameObject found for player {username}");
        }
    }

    public void StartGame() {
        gameStarted = true;
        BackendConnector.Instance?.SendGameStart(); // ✅ Important!
        Debug.Log("📤 Triggered SendGameStart from PlayerTracker");

        Initializer.ActiveGameLogic?.OnStartGame();
        Debug.Log("▶️ Game Started");
    }




    public void EndGame() {
        gameStarted = false;
        Initializer.ActiveGameLogic?.OnEndGame();
        Debug.Log("🛑 Game Ended");

        foreach (var obj in playerObjects.Values) {
            Destroy(obj);
        }

        playerObjects.Clear();

        GameSessionSaver.Instance?.SaveEnd();
    }

    public bool TryGetPlayer(string username, out GameObject playerObj) {
        return playerObjects.TryGetValue(username, out playerObj);
    }

    // ✅ Local Player Identity API

    public void SetLocalPlayerUsername(string username) {
        localPlayerUsername = username;
    }

    public string GetLocalPlayerUsername() => localPlayerUsername;

    public bool IsLocal(string username) => username == localPlayerUsername;
}
