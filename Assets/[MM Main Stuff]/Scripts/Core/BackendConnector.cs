using UnityEngine;
using NativeWebSocket;
using System;
using System.Text;
using System.Threading.Tasks;
using TMPro;

[Serializable]
public class SessionInitData {
    public string code;
    public string type;
    public string location;
    public int allowedNumberOfPlayers;
    public string filename;
}

[Serializable]
public class WebMessageBase {
    public string type;
}

[Serializable]
public class PlayerJoinMessage {
    public string type;
    public string username;
    public string team;
}

[Serializable]
public class PlayerActionMessage {
    public string type;
    public string username;
    public string action;
}

[Serializable]
public class RoleMessage {
    public string type;
    public string username;
    public string role;
    public string code;
}

[Serializable]
public class GameStartSignal {
    public string type = "gameStart";
    public string code;
    public string gameType;
    public string location;
    public string timestamp;
}

[Serializable]
public class GameEndSignal {
    public string type = "gameEnd";
    public string code;
    public string gameType;
    public string location;
    public string timestamp;
    public string[] players;
}

public class BackendConnector : MonoBehaviour {
    public static BackendConnector Instance;
    private WebSocket websocket;

    public string gameCode;
    public string gameType;
    public string location;

    [Header("UI")]                             // NEW
    [SerializeField] private TMP_Text codeText; // NEW – drag your TMP_Text here in Inspector


    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public async void Connect(SessionInitData sessionData) {
        gameCode = sessionData.code;
        gameType = sessionData.type;
        location = sessionData.location;


        if (codeText != null) codeText.text = gameCode;

        websocket = new WebSocket("wss://cinemagame.duckdns.org/ws/");

        websocket.OnOpen += () => {
            Debug.Log("🟢 WebSocket connected");
            SendSessionInfo(sessionData);
        };

        websocket.OnError += (e) => Debug.LogError("❌ WebSocket error: " + e);
        websocket.OnClose += (e) => Debug.Log("🔴 WebSocket closed");

        websocket.OnMessage += (bytes) => {
            string msg = Encoding.UTF8.GetString(bytes);
            Debug.Log("📥 WS Message: " + msg);

            HandleIncomingMessage(msg);
        };

        await websocket.Connect();
    }

    private void HandleIncomingMessage(string msg) {
        var baseMsg = JsonUtility.FromJson<WebMessageBase>(msg);

        switch (baseMsg.type) {
            case "playerJoin":
                var join = JsonUtility.FromJson<PlayerJoinMessage>(msg);
                PlayerTracker.Instance.RegisterPlayer(join.username, join.team);
                break;

            case "action":
                var action = JsonUtility.FromJson<PlayerActionMessage>(msg);
                if (PlayerTracker.Instance.gameStarted) {
                    PlayerTracker.Instance.HandleAction(action.username, action.action);
                }
                break;

            case "roleAssignment":
                var role = JsonUtility.FromJson<RoleMessage>(msg);
                Debug.Log($"🎭 Role for {role.username}: {role.role}");

                if (string.IsNullOrEmpty(PlayerTracker.Instance.GetLocalPlayerUsername()) && role.role == "player") {
                    PlayerTracker.Instance.SetLocalPlayerUsername(role.username);
                    Debug.Log($"✅ Local player assigned: {role.username}");
                }
                break;

            default:
                Debug.LogWarning($"⚠️ Unhandled message type: {baseMsg.type}");
                break;
        }
    }

    public async void SendSessionInfo(SessionInitData data) {
        string json = JsonUtility.ToJson(data);
        await websocket.SendText(json);
        Debug.Log("📤 Sent session to backend: " + json);
    }

    public async void SendGameStart() {
        await Task.Delay(1000); // ✅ Delay to allow roleAssignment to propagate

        var msg = new GameStartSignal {
            code = gameCode,
            gameType = gameType,
            location = location,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(msg);

        if (websocket != null && websocket.State == WebSocketState.Open) {
            await websocket.SendText(json);
            Debug.Log("📤 Sent game start: " + json);
        } else {
            Debug.LogWarning("⚠️ WebSocket not open. Failed to send gameStart.");
        }
    }


    public async void SendGameEnd(string[] playerIds) {
        var msg = new GameEndSignal {
            code = gameCode,
            gameType = gameType,
            location = location,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            players = playerIds
        };
        string json = JsonUtility.ToJson(msg);
        await websocket.SendText(json);
        Debug.Log("📤 Sent game end: " + json);
    }

    public async void SendRaw(string json) {
        if (websocket != null && websocket.State == WebSocketState.Open) {
            await websocket.SendText(json);
            Debug.Log("📤 Sent raw to backend: " + json);
        } else {
            Debug.LogWarning("⚠️ WebSocket not open. Failed to send raw: " + json);
        }
    }
    public void SetRole(string username, string role) {
        var msg = new {
            type = "roleAssignment",
            username = username,
            role = role,
            code = gameCode
        };

        string json = JsonUtility.ToJson(msg);
        SendRaw(json);
    }


    public async void Close() {
        if (websocket != null && websocket.State == WebSocketState.Open) {
            await websocket.Close();
        }
    }

    void Update() {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    void OnApplicationQuit() => Close();
}
