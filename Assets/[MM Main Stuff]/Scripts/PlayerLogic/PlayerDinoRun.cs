using UnityEngine;

public class PlayerDinoRun : MonoBehaviour {
    public PlayerAvatarRunner avatar;
    public string username;
    public bool isSpectator = false;

    private int moveLeft = 0;
    private int moveRight = 0;
    private int jump = 0;

    public void HandleAction(string action) {
        if (isSpectator) {
            Debug.Log($"[DinoRun] {username} is a spectator — action ignored.");
            return;
        }

        switch (action) {
            case "moveLeft": moveLeft++; break;
            case "moveRight": moveRight++; break;
            case "jump": jump++; break;
        }

        avatar?.Move(action);
    }
}
