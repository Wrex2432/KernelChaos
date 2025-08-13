using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DinoRunLogic : IGameLogicHandler
{
    private GameObject logicPrefab;
    private GameObject avatarPrefab;

    private List<string> allUsernames = new();
    private HashSet<string> selectedPlayers = new();
    private int numberToPick = 1;

    public DinoRunLogic(GameObject logicPrefab, GameObject avatarPrefab)
    {
        this.logicPrefab = logicPrefab;
        this.avatarPrefab = avatarPrefab;

        string path = Path.Combine(Application.dataPath, "..", "control.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<ControlData>(json);
            numberToPick = data.dinorunNumberOfPlayerPicked;
        }
    }

    public GameObject SpawnPlayer(string username, string team = null)
    {
        GameObject obj = Object.Instantiate(logicPrefab);
        obj.name = "Player_" + username;
        obj.tag = "Player";

        var script = obj.GetComponent<PlayerDinoRun>();
        if (script == null) script = obj.AddComponent<PlayerDinoRun>();

        script.username = username;
        script.isSpectator = true;

        return obj;
    }

    public void HandleAction(GameObject playerObject, string action)
    {
        var script = playerObject.GetComponent<PlayerDinoRun>();
        if (script == null) return;

        script.HandleAction(action);
    }

    public void OnPlayerJoin(string username, string team = null)
    {
        if (!allUsernames.Contains(username))
        {
            allUsernames.Add(username);
            Debug.Log($"[DinoRunLogic] Player joined: {username}");
        }
    }

    public void OnStartGame()
    {
        Debug.Log($"[DinoRunLogic] Game Started — selecting {numberToPick} players");

        selectedPlayers = new HashSet<string>(
            allUsernames.OrderBy(_ => Random.value).Take(numberToPick)
        );

        Transform spawnPoint = Initializer.Instance.avatarSpawnPoint_Small;
        if (selectedPlayers.Count > 4 && Initializer.Instance.avatarSpawnPoint_Large != null)
        {
            spawnPoint = Initializer.Instance.avatarSpawnPoint_Large;
        }

        Vector3 origin = spawnPoint ? spawnPoint.position : Vector3.zero;
        Vector3 spacing = new Vector3(2f, 0f, 0f);
        DinoRunSpawner.ConfigureSpawn(origin, spacing);

        DinoRunSpawner.SpawnPlaceholders(selectedPlayers.Count, avatarPrefab);
        DinoRunSpawner.AssignPlayers(selectedPlayers.ToList(), allUsernames);

        foreach (string username in allUsernames)
        {
            string expectedName = "Player_" + username;
            GameObject obj = GameObject.Find(expectedName);

            if (obj == null)
            {
                // 🛑 Still missing? Create it now.
                obj = Object.Instantiate(logicPrefab);
                obj.name = expectedName;
            }

            var script = obj.GetComponent<PlayerDinoRun>();
            if (script == null) script = obj.AddComponent<PlayerDinoRun>();

            script.username = username;
            script.isSpectator = !selectedPlayers.Contains(username);

            // Assign avatar runner if it exists
            var avatarRunner = obj.GetComponent<PlayerAvatarRunner>();
            if (avatarRunner != null)
            {
                avatarRunner.assignedUsername = username;
                avatarRunner.SetControl(true);
            }

            // ✅ Update backend roles mapping
            BackendConnector.Instance?.SetRole(username, script.isSpectator ? "spectator" : "player");

            var roleMsg = new RoleMessage
            {
                type = "roleAssignment",
                username = username,
                role = script.isSpectator ? "spectator" : "player",
                code = BackendConnector.Instance?.gameCode ?? "UNKNOWN"
            };

            string json = JsonUtility.ToJson(roleMsg);
            BackendConnector.Instance?.SendRaw(json);

            Debug.Log($"[DinoRunLogic] {username} → {(script.isSpectator ? "Spectator" : "Player")}");
        }
    }

    public void OnEndGame()
    {
        Debug.Log("[DinoRunLogic] Game Ended");
        allUsernames.Clear();
        selectedPlayers.Clear();
        DinoRunSpawner.Clear();
    }

    [System.Serializable]
    public class RoleMessage
    {
        public string type;
        public string username;
        public string role;
        public string code;
    }
}
