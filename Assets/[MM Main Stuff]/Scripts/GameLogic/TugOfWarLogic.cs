using UnityEngine;

public class TugOfWarLogic : IGameLogicHandler {
    private GameObject prefab;
    private BallMover ballMover;

    public TugOfWarLogic(GameObject prefab) {
        this.prefab = prefab;

        GameObject ball = GameObject.Find("Ball");
        if (ball != null) {
            ballMover = ball.GetComponent<BallMover>();
        }
    }

    public GameObject SpawnPlayer(string username, string team = null) {
        GameObject obj = Object.Instantiate(prefab);
        obj.name = $"Player_{team}_{username}";
        obj.tag = "Player";

        var script = obj.GetComponent<PlayerTugOfWar>();
        if (script != null) {
            script.username = username;
            script.team = team;
        }

        return obj;
    }

    public void HandleAction(GameObject playerObject, string action) {
        var script = playerObject.GetComponent<PlayerTugOfWar>();
        if (script == null) return;

        script.HandleAction(action);

        if (action == "pull") {
            if (script.team == "TeamA") {
                ballMover?.MoveLeft();
            } else if (script.team == "TeamB") {
                ballMover?.MoveRight();
            }
        }
    }

    public void OnPlayerJoin(string username, string team = null) {
        Debug.Log($"[TugOfWarLogic] Player joined: {username} on {team}");
    }

    public void OnStartGame() {
        Debug.Log("[TugOfWarLogic] Game Started");
    }

    public void OnEndGame() {
        Debug.Log("[TugOfWarLogic] Game Ended");
    }
}
