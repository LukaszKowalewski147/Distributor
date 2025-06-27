using System;
using TMPro;
using UnityEngine;

public class PrepareMultiplayer : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;

    public void PrepareMultiplayerData()
    {
        PlayerData.playerID = GeneratePlayerID();
        PlayerData.playerName = playerNameInput.text;

        Debug.Log("PREPARE MULTIPLATER DATA");
        Debug.Log("PlayerData __ID: " + PlayerData.playerID + "\nPlayerData name: " + PlayerData.playerName);
        Debug.Log("PREPARE MULTIPLATER DATA END");
    }

    private string GeneratePlayerID()
    {
        int idLength = 12;
        string s = string.Empty;
        var random = new System.Random();

        for (int i = 0; i < idLength; i++)
            s = String.Concat(s, random.Next(10).ToString());
        return s;
    }
}
