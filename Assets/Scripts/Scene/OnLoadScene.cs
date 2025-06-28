using UnityEngine;

public class OnLoadScene : MonoBehaviour
{
    private void Awake()
    {
        GameObject otherPlayersObject = GameObject.FindWithTag("OtherPlayers");
        Debug.Log("otherPlayersObject: " + otherPlayersObject.ToString());

        PlayerData.otherPlayersManager = otherPlayersObject.GetComponent<OtherPlayersManager>();
        Debug.Log("otherPlayersManager: " + PlayerData.otherPlayersManager.ToString());
    }
}
