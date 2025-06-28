using UnityEngine;
using System.Collections.Generic;

public class OtherPlayersManager : MonoBehaviour
{
    private List<GameObject> otherPlayers = new List<GameObject>();

    public GameObject GetPlayer(string id)
    {
        foreach (var player in otherPlayers)
        {
            if (player.name == id)
                return player;
        }
        return null;
    }

    public void AddPlayer(GameObject player)
    {
        otherPlayers.Add(player);
    }

    public void RemovePlayer(GameObject player)
    {
        otherPlayers.Remove(player);
    }

    public bool IsPlayerRendered(string id)
    {
        foreach (var player in otherPlayers)
        {
            if (player.name == id)
                return true;
        }
        return false;
    }
}
