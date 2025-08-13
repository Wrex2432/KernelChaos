using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoEndGameOnLastPlayer : MonoBehaviour {
    public float checkInterval = 1f;
    public int alivePlayerCount = 0; // ✅ visible in Inspector

    private bool isGameRunning = false;

    public void BeginMonitoring () {
        isGameRunning = true;
        Debug.Log("🕵️ AutoEndGameOnLastPlayer: Monitoring started.");
    }

    void Start () {
        StartCoroutine(CheckLoop());
    }

    private IEnumerator CheckLoop () {
        while (true) {
            if (isGameRunning) {
                GameObject[] allTaggedPlayers = GameObject.FindGameObjectsWithTag("Player");

                List<GameObject> validPlayers = new List<GameObject>();
                foreach (var obj in allTaggedPlayers) {
                    if (obj.name.StartsWith("PlayerAvatar") && obj.activeInHierarchy) {
                        validPlayers.Add(obj);
                    }
                }

                alivePlayerCount = validPlayers.Count;

                if (alivePlayerCount <= 1) {
                    Debug.Log($"🛑 Auto-end triggered. Remaining valid players: {alivePlayerCount}");

                    if (alivePlayerCount == 1) {
                        Debug.Log($"🎉 Last player standing: {validPlayers[0].name}");
                    }

                    PlayerTracker.Instance?.EndGame();
                    isGameRunning = false;
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }
}
