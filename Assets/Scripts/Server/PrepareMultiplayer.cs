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
        PlayerData.zone = "forest";

        Debug.Log("PREPARE MULTIPLATER DATA");
        Debug.Log("ID: [" + PlayerData.playerID + "] name: [" + PlayerData.playerName + "] zone: [" + PlayerData.zone + "]");
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
