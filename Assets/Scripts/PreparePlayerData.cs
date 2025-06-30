using UnityEngine;

public class PreparePlayerData : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("PreparePlayerData.cs - Awake()");
        // Prepare all data needed to start the game in singleplayer mode
        PlayerData.playerID = "0000";
        PlayerData.playerName = "name";
        PlayerData.zone = "forest";
        PlayerData.hp = 100;
        PlayerData.lemonsCount = 0;
        PlayerData.cactusNeedlesCount = 0;
        PlayerData.multiplayer = false;
    }
}
