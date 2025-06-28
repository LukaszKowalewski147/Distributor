using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance { get; private set; }

    private IConnection connection;
    private IModel channel;
    private string playerId;
    private string playerName;
    private string zone;
    private Thread consumerThread;
    private volatile bool isRunning;

    // RabbitMQ configuration (matching server)
    private const string RABBITMQ_HOST = "localhost";
    private const string EXCHANGE_C2S = "game.client_to_server";
    private const string EXCHANGE_M2C = "game.movement_to_client";
    private const string EXCHANGE_A2C = "game.animations_to_client";
    private const string EXCHANGE_I2C = "game.interactions_to_client";
    private const string EXCHANGE_PT = "game.player_transfer";
    private const float TICK_INTERVAL = 0.1f; // 100ms, matching server

    // Queues and routing keys
    private string movementQueue;
    private string animationsQueue;
    private string interactionsQueue;
    private string transferQueue;

    void Awake()
    {
        #region Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion

        // Initialize player ID, name and zone
        playerId = PlayerData.playerID;
        playerName = PlayerData.playerName;
        zone = PlayerData.zone;

        // Initialize queue names
        movementQueue = $"movement.{playerId}";
        animationsQueue = $"animations.{playerId}";
        interactionsQueue = $"interactions.{playerId}";
        transferQueue = $"transfer.*.{zone}";
    }

    void Start()
    {
        ConnectToRabbitMQ();
        SendJoinMessage();
        StartSendingUpdates();
        StartConsuming();
    }

    void ConnectToRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory { HostName = RABBITMQ_HOST };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            // Declare exchanges
            channel.ExchangeDeclare(EXCHANGE_C2S, ExchangeType.Direct);
            channel.ExchangeDeclare(EXCHANGE_M2C, ExchangeType.Topic);
            channel.ExchangeDeclare(EXCHANGE_A2C, ExchangeType.Topic);
            channel.ExchangeDeclare(EXCHANGE_I2C, ExchangeType.Topic);
            channel.ExchangeDeclare(EXCHANGE_PT, ExchangeType.Topic);

            // Declare and bind client-specific queues
            channel.QueueDeclare(movementQueue, false, false, true, null);
            channel.QueueBind(movementQueue, EXCHANGE_M2C, movementQueue);

            channel.QueueDeclare(animationsQueue, false, false, true, null);
            channel.QueueBind(animationsQueue, EXCHANGE_A2C, animationsQueue);

            channel.QueueDeclare(interactionsQueue, false, false, true, null);
            channel.QueueBind(interactionsQueue, EXCHANGE_I2C, interactionsQueue);

            channel.QueueDeclare(transferQueue, false, false, true, null);
            channel.QueueBind(transferQueue, EXCHANGE_PT, transferQueue);

            Debug.Log("Connected to RabbitMQ and queues set up.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to RabbitMQ: {e.Message}");
        }
    }

    void SendJoinMessage()
    {
        var message = new { id = playerId };
        SendMessage(EXCHANGE_C2S, $"join.{zone}", message);
        Debug.Log("SERVER - Sending a JOIN MESSAGE:\n" + message.ToString());
    }

    void SendLeaveMessage()
    {
        Debug.Log("SendLeaveMessage");
        var message = new { id = playerId };
        SendMessage(EXCHANGE_C2S, $"leave.{zone}", message);
        Debug.Log("SERVER - Sending a LEAVE MESSAGE:\n" + message.ToString());
    }

    void SendMessage(string exchange, string routingKey, object message)
    {
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish(exchange, routingKey, null, body);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message to {routingKey}: {e.Message}");
        }
    }

    void StartSendingUpdates()
    {
        InvokeRepeating(nameof(SendPlayerUpdate), 0f, TICK_INTERVAL);
    }

    void SendPlayerUpdate()
    {
        Vector3 position = PlayerData.position;

        var message = new
        {
            id = playerId,
            posX = position.x,
            posY = position.y,
            posZ = position.z,
            rotY = PlayerData.rotationY,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"movement.{zone}", message);
        //Debug.Log("SERVER - Sending a MOVEMENT UPDATE MESSAGE:\n" + message.ToString());
    }

    public void SendAnimation(string animation)
    {
        var message = new
        {
            playerId = playerId,
            animation,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"animations.{zone}", message);
        //Debug.Log("SERVER - Sending a ANIMATION TRIGGER MESSAGE:\n" + message.ToString());
    }
    
    public void SendInteraction(string interaction)
    {
        var message = new
        {
            playerId = playerId,
            interaction,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"interactions.{zone}", message);
        //Debug.Log("SERVER - Sending a INTERACTION TRIGGER MESSAGE:\n" + message.ToString());
    }
    
    public void SendPlayerTransfer(string targetZone)
    {
        var message = new
        {
            playerId = playerId,
            from = zone,
            to = targetZone,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"transfer.{zone}", message);
        Debug.Log("SERVER - Sending a PLAYER TRANSFER MESSAGE:\n" + message.ToString());

        PlayerData.zone = targetZone; // Update local zone
        zone = targetZone; 

        UpdateQueueBindings();
    }
    
    void UpdateQueueBindings()
    {
        // Unbind and delete old transfer queue
        channel.QueueUnbind(transferQueue, EXCHANGE_PT, transferQueue, null);
        channel.QueueDelete(transferQueue);

        // Update transfer queue for new zone
        transferQueue = $"transfer.*.{zone}";
        channel.QueueDeclare(transferQueue, false, false, true, null);
        channel.QueueBind(transferQueue, EXCHANGE_PT, transferQueue);
    }
    
    void StartConsuming()
    {
        isRunning = true;
        consumerThread = new Thread(ConsumeMessages);
        consumerThread.IsBackground = true;
        consumerThread.Start();
    }

    void ConsumeMessages()
    {
        try
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    if (routingKey.StartsWith("movement."))
                    {
                        ProcessMovementMessage(message);
                    }
                    else if (routingKey.StartsWith("animations."))
                    {
                        ProcessAnimationMessage(message);
                    }
                    else if (routingKey.StartsWith("interactions."))
                    {
                        ProcessInteractionMessage(message);
                    }
                    else if (routingKey.StartsWith("transfer."))
                    {
                        ProcessTransferMessage(message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing message: {e.Message}");
                }
            };

            channel.BasicConsume(movementQueue, true, consumer);
            channel.BasicConsume(animationsQueue, true, consumer);
            channel.BasicConsume(interactionsQueue, true, consumer);
            channel.BasicConsume(transferQueue, true, consumer);

            while (isRunning)
            {
                Thread.Sleep(10); // Prevent tight loop
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Consumer thread error: {e.Message}");
        }
    }

    void ProcessMovementMessage(string message)
    {
        //Debug.Log("SERVER - movement response message: " + message);
        var data = JsonSerializer.Deserialize<MovementMessage>(message);
        if (data?.Updates != null)
        {
            foreach (var update in data.Updates)
            {
                if (update.Id != playerId) // Update other players
                {
                    
                }
                else
                {
                    // My position for debugging
                    //Debug.Log($"Me: X={update.Position.X}, Y={update.Position.Y}, Z={update.Position.Z}, rotY={update.RotationY}");
                }
            }
        }
    }

    void ProcessAnimationMessage(string message)
    {
        var data = JsonSerializer.Deserialize<AnimationMessage>(message);
        //Debug.Log($"Animation from {data.PlayerId}: animation: {data.Animation}");
        // Handle animation (e.g., play animation on other player)
    }

    void ProcessInteractionMessage(string message)
    {
        var data = JsonSerializer.Deserialize<InteractionMessage>(message);
        //Debug.Log($"Interaction from {data.PlayerId}: {data.Interaction}");
        // Handle interaction (e.g., update UI or trigger events)
    }
    
    void ProcessTransferMessage(string message)
    {
        var data = JsonSerializer.Deserialize<TransferMessage>(message);
        if (data.PlayerId == playerId)
        {
            zone = data.To;
            Debug.Log($"Player transferred to {zone}");
        }
    }

    #region JSON messages
    [Serializable]
    private class MovementMessage
    {
        [JsonPropertyName("updates")]
        public List<PlayerUpdate> Updates { get; set; }
    }

    [Serializable]
    private class PlayerUpdate
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
    private class Position
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
    private class AnimationMessage
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("animation")]
        public string Animation { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }

    [Serializable]
    private class InteractionMessage
    {
        [JsonPropertyName("playerId")]
        public string PlayerId { get; set; }

        [JsonPropertyName("interaction")]
        public string Interaction { get; set; }

        [JsonPropertyName("timestamp")]
        public float Timestamp { get; set; }
    }

    [Serializable]
    private class TransferMessage
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
    #endregion

    private void OnApplicationQuit()
    {
        Debug.Log("OnAppQuit");
        SendLeaveMessage();
        isRunning = false;
        channel?.Close();
        connection?.Close();
    }
}