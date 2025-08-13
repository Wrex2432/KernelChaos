using UnityEngine;

public interface IGameLogicHandler {
    GameObject SpawnPlayer(string username, string team = null);
    void HandleAction(GameObject playerObject, string action);
    void OnPlayerJoin(string username, string team = null);
    void OnStartGame();
    void OnEndGame();
}
