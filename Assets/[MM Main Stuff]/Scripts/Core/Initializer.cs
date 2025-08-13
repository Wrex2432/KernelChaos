using UnityEngine;
using System;
using System.IO;

public class Initializer : MonoBehaviour {
    public static Initializer Instance;
    public static IGameLogicHandler ActiveGameLogic;
    public static ControlData controlData;

    // Existing prefabs
    public GameObject dinoRunPrefab;
    public GameObject dinoAvatarPrefab;
    public GameObject tugOfWarPrefab;

    // [KC] Kernel Chaos prefab (assign in Inspector)
    public GameObject kernelChaosPrefab;

    // Optional: used elsewhere in your project
    public Transform avatarSpawnPoint_Small; // ≤ 4 players
    public Transform avatarSpawnPoint_Large; // > 4 players

    // Local-only toggle so we can skip the backend while prototyping
    public bool connectToBackend = true;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        controlData = LoadControlFile();
        if (controlData == null) return;

        // Register prefabs with the registry
        GameLogicRegistry.dinoPrefab = dinoRunPrefab;
        GameLogicRegistry.dinoAvatarPrefab = dinoAvatarPrefab;
        GameLogicRegistry.tugPrefab = tugOfWarPrefab;

        // [KC]
        GameLogicRegistry.kernelChaosPrefab = kernelChaosPrefab;

        // Create logic handler
        ActiveGameLogic = GameLogicRegistry.Get(controlData.gameType);
        if (ActiveGameLogic == null) {
            Debug.LogError($"❌ Unsupported game type: {controlData.gameType}");
            return;
        }

        Debug.Log($"✅ Game initialized: {controlData.gameType}");

        // Generate code and save session
        string code = GenerateCode(4);
        GameSessionSaver.Instance?.SaveStart(controlData, code);

        var sessionData = new SessionInitData {
            code = code,
            type = controlData.gameType,
            location = controlData.location,
            allowedNumberOfPlayers = controlData.allowedNumberOfPlayers,
            filename = GameSessionSaver.Instance?.GetFilename()
        };

        if (connectToBackend) {
            BackendConnector.Instance?.Connect(sessionData);
        } else {
            Debug.Log("🟡 Skipping backend connect (local-only test).");
        }
    }

    private ControlData LoadControlFile() {
        string path = Path.Combine(Application.dataPath, "..", "control.json");
        if (!File.Exists(path)) {
            Debug.LogError("❌ control.json not found at: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ControlData>(json);
    }

    private string GenerateCode(int length) {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        System.Random rand = new();
        char[] result = new char[length];
        for (int i = 0; i < length; i++) result[i] = chars[rand.Next(chars.Length)];
        return new string(result);
    }
}
