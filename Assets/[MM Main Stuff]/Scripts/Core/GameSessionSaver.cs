using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class GameSessionSaveFormat {
    public string code;
    public string type;
    public string location;
    public int allowedNumberOfPlayers;
    public string timestampStart;
    public string timestampEnd;
    public int totalPlayersJoined;
    public string filename;
    public Dictionary<string, string> players = new();
    public Dictionary<string, List<string>> teams = new();
}

public class GameSessionSaver : MonoBehaviour {
    public static GameSessionSaver Instance;

    private string savePath;
    private GameSessionSaveFormat data;
    private string location;
    private string gameType;
    private string timestampStartRaw;

    public int MaxPlayers => data?.allowedNumberOfPlayers ?? 0;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SaveStart(ControlData control, string code) {
        location = control.location;
        gameType = control.gameType;

        timestampStartRaw = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = timestampStartRaw + ".json";

        data = new GameSessionSaveFormat {
            code = code,
            type = gameType,
            location = location,
            allowedNumberOfPlayers = control.allowedNumberOfPlayers,
            timestampStart = timestampStartRaw,
            filename = filename
        };

        string folder = Path.Combine(Application.persistentDataPath, $"cinemagame_{location}", gameType);
        Directory.CreateDirectory(folder);
        savePath = Path.Combine(folder, filename);

        Debug.Log("📁 Saving session start to: " + savePath);
        Save();
    }

    public void RegisterPlayer(string username) {
        // Check if the game is team-based
        string team = null;

        if (Initializer.ActiveGameLogic != null && Initializer.ActiveGameLogic is TugOfWarLogic) {
            if (PlayerTracker.Instance.TryGetPlayer(username, out GameObject playerObj)) {
                var tug = playerObj.GetComponent<PlayerTugOfWar>();
                if (tug != null) {
                    team = tug.team;
                }
            }
        }

        if (!string.IsNullOrEmpty(team)) {
            if (!data.teams.ContainsKey(team)) {
                data.teams[team] = new List<string>();
            }

            if (!data.teams[team].Contains(username)) {
                data.teams[team].Add(username);
            }
        } else {
            if (!data.players.ContainsValue(username)) {
                string key = "player" + (data.players.Count + 1);
                data.players[key] = username;
            }
        }

        Save();
    }

    public void Save() {
        if (data == null || string.IsNullOrEmpty(savePath)) return;

        data.totalPlayersJoined = data.type == "tug_of_war"
            ? (data.teams.ContainsKey("TeamA") ? data.teams["TeamA"].Count : 0) +
              (data.teams.ContainsKey("TeamB") ? data.teams["TeamB"].Count : 0)
            : data.players.Count;

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(savePath, json);
        Debug.Log("💾 Saved session: " + savePath);
    }

    public void SaveEnd() {
        if (data == null) return;
        data.timestampEnd = DateTime.UtcNow.ToString("o");
        Save();
    }

    public string GetFilename() => data?.filename ?? "fallback.json";
}
