using UnityEngine;

public class SpawnOtherPlayer : MonoBehaviour
{
    public GameObject otherPlayer;

    private OtherPlayersManager otherPlayersManager;

    private void Start()
    {
        otherPlayersManager = GetComponent<OtherPlayersManager>();
    }

    public void Spawn(string ID)
    {
        GameObject newPlayer = Instantiate(otherPlayer, new Vector3(0, 0, 0), Quaternion.identity, transform);
        newPlayer.name = ID;

        otherPlayersManager.AddPlayer(newPlayer);
    }
}