using UnityEngine;
using UnityEngine.InputSystem;

public class GameControlInput : MonoBehaviour {
    private int devAvatarSpawnCount = 0;
    private Vector3 devSpacing = new Vector3(2f, 0f, 0f);

    public GameObject UIDisable;
    public GameObject qrImage;
    public Animator animator;

    void Update () {
        if (Keyboard.current.qKey.wasPressedThisFrame) {
            PlayerTracker.Instance.StartGame();
            UIDisable.SetActive(false);
            qrImage.SetActive(false);
            if (animator != null) {
                animator.SetBool("GameRunning", true);
            }

            // ✅ Start automatic end-game watcher
            FindObjectOfType<AutoEndGameOnLastPlayer>()?.BeginMonitoring();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame) {
            PlayerTracker.Instance.EndGame();
        }

        if (Keyboard.current.vKey.wasPressedThisFrame) {
            Debug.Log($"[Debug] Spawning test avatar #{devAvatarSpawnCount + 1}");

            if (Initializer.Instance != null) {
                GameObject avatarPrefab = Initializer.Instance.dinoAvatarPrefab;

                // Choose spawn point
                Transform spawnPoint = Initializer.Instance.avatarSpawnPoint_Small;
                if (devAvatarSpawnCount >= 4 && Initializer.Instance.avatarSpawnPoint_Large != null) {
                    spawnPoint = Initializer.Instance.avatarSpawnPoint_Large;
                }

                Vector3 origin = spawnPoint != null ? spawnPoint.position : Vector3.zero;
                Vector3 spawnPos = origin + devSpacing * devAvatarSpawnCount;

                GameObject obj = Instantiate(avatarPrefab, spawnPos, Quaternion.identity);

                var avatar = obj.GetComponent<PlayerAvatarRunner>();
                if (avatar == null) avatar = obj.AddComponent<PlayerAvatarRunner>();

                avatar.assignedUsername = $"DevTest_{devAvatarSpawnCount + 1}";
                obj.name = $"Player_{avatar.assignedUsername}"; // ✅ prevents duplicate creation

                avatar.SetControl(true);
                PlayerTracker.Instance.RegisterPlayer(avatar.assignedUsername, null);

                devAvatarSpawnCount++;
            } else {
                Debug.LogWarning("⚠️ Initializer.Instance is null — cannot spawn dev avatar");
            }
        }
    }
}
