using UnityEngine;
using System.Collections.Generic;

public class OtherPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    private List<GameObject> otherPlayers = new();

    private Vector3 defaultSpawnPositionLemonWorld = new(-89.0f, 0.2f, -28.0f);
    private float defaultSpawnRotationYLemonWorld = 23.0f;
    private Vector3 defaultSpawnPositionCactusWorld = new(-15.0f, 9.6f, -10.0f);
    private float defaultSpawnRotationYCactusWorld = 70.0f;

    public GameObject GetPlayer(string id)
    {
        foreach (var player in otherPlayers)
        {
            if (player.name == id)
                return player;
        }
        return null;
    }

    public void RemovePlayer(string id)
    {
        Debug.Log("OtherPlayers.cs - RemovePlayer(" + id + ")");
        // Prevent removing a non-existant player
        if (!IsPlayerRendered(id))
            return;

        GameObject playerToRemove = GetPlayer(id);
        otherPlayers.Remove(playerToRemove);
        Debug.Log("OtherPlayers.cs - RemovePlayer(" + id + "): removed from otherPlayers list");
        Destroy(playerToRemove);
        Debug.Log("OtherPlayers.cs - RemovePlayer(" + id + "): destroyed object");
    }

    public bool IsPlayerRendered(string id)
    {
       // Debug.Log("OtherPlayers.cs - IsPlayerRendered(): otherPlayers.Count: " + otherPlayers.Count.ToString());
        foreach (var player in otherPlayers)
        {
            //Debug.Log("OtherPlayers.cs - IsPlayerRendered(): otherPlayer ID: " + player.GetComponent<OtherPlayerManager>().GetID());
            if (player.name == id)
                return true;
        }
        return false;
    }

    public void ManageServerMovementUpdates(List<MessageClasses.PlayerUpdate> updates)
    {
        string myID = PlayerData.playerID;

        foreach (var update in updates)
        {
            if (update.Id != myID) // Update other players
            {
                string otherPlayerID = update.Id;

                //Debug.Log("movement server message - other player - id: " + otherPlayerID);

                if (IsPlayerRendered(otherPlayerID))
                {
                    //Debug.Log("movement update - other player - id: " + otherPlayerID);

                    OtherPlayerManager otherPlayerManager = GetPlayer(otherPlayerID).GetComponent<OtherPlayerManager>();

                    otherPlayerManager.MovePlayer(update.Position, update.RotationY);
                }
                else
                {
                    Debug.Log("OtherPlayers.cs - ManageServerMovementUpdates(): new player to spawn - id: " + otherPlayerID);

                    SpawnPlayer(otherPlayerID, update.Position, update.RotationY);
                }
            }
            else
            {
                // My position for debugging
                //Debug.Log($"Me: X={update.Position.X}, Y={update.Position.Y}, Z={update.Position.Z}, rotY={update.RotationY}");
            }
        }
    }

    public void ManageServerAnimationTrigger(MessageClasses.AnimationMessage message)
    {
        string otherPlayerID = message.PlayerId;

        //Debug.Log("animation server message - other player ID: " + otherPlayerID);

        if (IsPlayerRendered(otherPlayerID))
        {
            //Debug.Log("animation update player: " + otherPlayerID);

            OtherPlayerManager otherPlayerManager = GetPlayer(otherPlayerID).GetComponent<OtherPlayerManager>();

            otherPlayerManager.TriggerAnimation(AnimationManager.ConvertAnimationStringToEnum(message.Animation));
        }
        else
        {
            Debug.Log("animation new player to spawn - ID: " + otherPlayerID);

            SpawnPlayer(otherPlayerID);
        }
    }
    private void SpawnPlayer(string ID)
    {
        // Prevent spawning the same player multiple times due to server delay
        if (IsPlayerRendered(ID))
            return;

        string zone = PlayerData.zone;
        Vector3 position;
        float rotationY;

        switch (zone)
        {
            case "forest":
                position = defaultSpawnPositionLemonWorld;
                rotationY = defaultSpawnRotationYLemonWorld;
                break;
            case "desert":
                position = defaultSpawnPositionCactusWorld;
                rotationY = defaultSpawnRotationYCactusWorld;
                break;
            default:
                position = defaultSpawnPositionLemonWorld;
                rotationY = defaultSpawnRotationYLemonWorld;
                break;
        }
        InstantiatePlayer(ID, position, rotationY);
    }

    private void SpawnPlayer(string ID, Vector3 position, float rotationY)
    {
        // Prevent spawning the same player multiple times due to server delay
        if (IsPlayerRendered(ID))
            return;

        InstantiatePlayer(ID, position, rotationY);
    }

    private void InstantiatePlayer(string ID, Vector3 position, float rotationY)
    {
        Debug.Log("OtherPlayers.cs - InstantiatePlayer(): spawn player " + ID);

        GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.Euler(0.0f, rotationY, 0.0f), transform);
        OtherPlayerManager playerManager = newPlayer.GetComponent<OtherPlayerManager>();
        playerManager.SetID(ID);
        newPlayer.name = ID;

        Debug.Log("Player " + ID + " spawned");

        otherPlayers.Add(newPlayer);
    }
}
