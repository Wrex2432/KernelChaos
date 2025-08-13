using UnityEngine;
using System.Collections.Generic;

public static class DinoRunSpawner {
    private static List<PlayerAvatarRunner> avatars = new();

    // Internal configurable spawn settings
    private static Vector3 internalSpawnOrigin = Vector3.zero;
    private static Vector3 internalSpacing = new Vector3(2f, 0f, 0f);

    public static void ConfigureSpawn(Vector3 origin, Vector3 spacing) {
        internalSpawnOrigin = origin;
        internalSpacing = spacing;
    }

    public static void SpawnPlaceholders(int count, GameObject prefab) {
        Clear();

        for (int i = 0; i < count; i++) {
            Vector3 pos = internalSpawnOrigin + internalSpacing * i;
            GameObject obj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
            obj.name = $"PlayerAvatar_{i + 1}";
            obj.tag = "Player"; // Optional tag or "Placeholder" if needed

            var avatar = obj.GetComponent<PlayerAvatarRunner>();
            if (avatar == null) avatar = obj.AddComponent<PlayerAvatarRunner>();

            avatar.assignedUsername = null; // ✅ used to mark as placeholder
            avatar.SetControl(false);
            avatars.Add(avatar);
        }
    }

    public static void AssignPlayers(List<string> selectedUsernames, List<string> allUsernames) {
        for (int i = 0; i < avatars.Count; i++) {
            if (i < selectedUsernames.Count) {
                string username = selectedUsernames[i];
                var avatar = avatars[i];

                avatar.assignedUsername = username;
                avatar.SetControl(true);
                avatar.gameObject.SetActive(true);

                GameObject logicObj = GameObject.Find("Player_" + username);
                if (logicObj != null) {
                    var logic = logicObj.GetComponent<PlayerDinoRun>();
                    if (logic != null) {
                        logic.avatar = avatar;
                    }
                }

                Debug.Log($"🧩 Assigned {username} to avatar at {avatar.transform.position}");
            } else {
                avatars[i].gameObject.SetActive(false);
            }
        }
    }

    public static void Clear() {
        foreach (var avatar in avatars) {
            if (avatar != null && avatar.gameObject != null && avatar.assignedUsername == null) {
                GameObject.Destroy(avatar.gameObject); // ✅ only destroy placeholders
            }
        }
        avatars.Clear();
    }
}
