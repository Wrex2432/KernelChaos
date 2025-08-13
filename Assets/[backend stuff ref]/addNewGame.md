## How to Add a New Game Type to the Unity + Web + Backend System

### ✨ Overview

To add a new game type (e.g., `"button_smash"`), follow this structured process across all components: Unity, Web, Backend.

---

### 📁 Step 1: Update `control.json`

```json
{
  "gameType": "button_smash",
  "allowedNumberOfPlayers": 4,
  "location": "local"
}
```

---

### 🟦 Step 2: Unity – Add New Logic Script

Create a new logic class implementing `IGameLogicHandler`:

```csharp
// ButtonSmashLogic.cs
public class ButtonSmashLogic : IGameLogicHandler
{
    public void OnPlayerJoin(string username, string team)
    {
        Debug.Log($"[ButtonSmashLogic] Player joined: {username}");
        PlayerInitializer.Instance.RegisterPlayer(username);
    }

    public void OnStartGame() => Debug.Log("[ButtonSmashLogic] Game Started");
    public void OnEndGame() => Debug.Log("[ButtonSmashLogic] Game Ended");

    public void OnAction(string username, string action)
    {
        Debug.Log($"[ButtonSmashLogic] {username} did: {action}");
        PlayerInitializer.Instance.TriggerButtonSmashAction(username, action);
    }
}
```

Also:

* Create `PlayerButtonSmash.cs`
* Add player prefab in Unity

---

### 🔄 Step 3: Update `Initializer.cs`

Add your new game logic to the startup switch:

```csharp
switch (control.gameType)
{
    case "dino_run":
        ActiveGameLogic = new DinoRunLogic(); break;
    case "tug_of_war":
        ActiveGameLogic = new TugOfWarLogic(); break;
    case "button_smash":
        ActiveGameLogic = new ButtonSmashLogic(); break;
    default:
        Debug.LogError("❌ Unsupported game type: " + control.gameType); return;
}
```

---

### 🧱 Step 4: Extend `PlayerInitializer.cs`

Add logic for instantiating and handling the new player type:

```csharp
public GameObject buttonSmashPrefab;

public GameObject CreatePlayerButtonSmash(string username)
{
    GameObject obj = Instantiate(buttonSmashPrefab);
    obj.name = "Player_" + username;
    var script = obj.GetComponent<PlayerButtonSmash>();
    if (script != null) script.username = username;
    return obj;
}

public void TriggerButtonSmashAction(string username, string action)
{
    var player = GameObject.Find("Player_" + username)?.GetComponent<PlayerButtonSmash>();
    if (player != null) player.HandleAction(action);
}
```

---

### 🌐 Step 5: Backend Support

Create `/games/button_smash.js` similar to `dino_run.js`. Then update `server.js`:

```js
const buttonSmash = require('./games/button_smash');

const gameHandlers = {
  dino_run: dinoRun,
  tug_of_war: tugOfWar,
  button_smash: buttonSmash
};
```

---

### 🎮 Step 6: Web App

Create a new folder or HTML page for `button_smash`:

* Connect using `gameType: "button_smash"`
* Trigger actions with `/trigger`
* Follow existing UI patterns

---

### ✅ Summary

| Layer          | Action Required                                                      |
| -------------- | -------------------------------------------------------------------- |
| Unity          | Add logic class, player script, prefab, support in PlayerInitializer |
| `control.json` | Add new gameType and player count                                    |
| Backend        | Create handler, register in `gameHandlers`                           |
| Web App        | Add new connection + trigger UI and logic                            |

Let us know your game rules if you want help scaffolding it further!
