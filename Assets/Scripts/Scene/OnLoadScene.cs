using UnityEngine;

public class OnLoadScene : MonoBehaviour
{
    private void Awake()
    {
        bool multiplayer = PlayerData.multiplayer;
        Debug.Log("OnLoadScene.cs - Awake(): multiplayer: " + multiplayer.ToString());

        if (multiplayer)
        {
            GameObject otherPlayersObject = GameObject.FindWithTag("OtherPlayers");
            PlayerData.otherPlayers = otherPlayersObject.GetComponent<OtherPlayers>();
        }
    }
}
