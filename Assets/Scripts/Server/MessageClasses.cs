using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

public static class MessageClasses
{
    [Serializable]
    public class MovementMessage
    {
        [JsonPropertyName("updates")]
        public List<PlayerUpdate> Updates { get; set; }
    }

    [Serializable]
    public class PlayerUpdate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("position")]
        public Position Position { get; set; }

        [JsonPropertyName("rotationY")]
        public float RotationY { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }

    [Serializable]
    public class Position
    {
        [JsonPropertyName("x")]
        public float X { get; set; }

        [JsonPropertyName("y")]
        public float Y { get; set; }

        [JsonPropertyName("z")]
        public float Z { get; set; }

        // Konwersja na Vector3
        public static implicit operator Vector3(Position pos) => new Vector3(pos.X, pos.Y, pos.Z);
    }

    [Serializable]
    public class AnimationMessage
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("animation")]
        public string Animation { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }

    [Serializable]
    public class InteractionMessage
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("interaction")]
        public string Interaction { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }

    [Serializable]
    public class TransferMessage
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }
}