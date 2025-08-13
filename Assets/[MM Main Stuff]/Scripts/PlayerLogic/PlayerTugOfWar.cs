using UnityEngine;

public class PlayerTugOfWar : MonoBehaviour {
    public string username;
    public string team;

    private int pulls = 0;

    public void HandleAction(string action) {
        if (action == "pull") {
            pulls++;
            Debug.Log($"[TugOfWar] {username} ({team}) pulled → {pulls}");
        } else {
            Debug.LogWarning($"[TugOfWar] Unknown action '{action}' from {username}");
        }
    }
}
