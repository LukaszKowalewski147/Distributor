using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private PlayerStats playerStats;
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
    private string interactionsQueue;
    private string animationsQueue;
    private string transferQueue;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on Player GameObject!");
            enabled = false;
            return;
        }

        // Initialize player ID and zone
        playerId = PlayerData.playerID;
        playerName = PlayerData.playerName;
        zone = PlayerData.zone;

        // Initialize queue names
        movementQueue = $"movement.{playerId}";
        interactionsQueue = $"interactions.{playerId}";
        animationsQueue = $"animations.{playerId}";
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

            channel.QueueDeclare(interactionsQueue, false, false, true, null);
            channel.QueueBind(interactionsQueue, EXCHANGE_I2C, interactionsQueue);

            channel.QueueDeclare(animationsQueue, false, false, true, null);
            channel.QueueBind(animationsQueue, EXCHANGE_A2C, animationsQueue);

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
    }

    void SendLeaveMessage()
    {
        var message = new { id = playerId };
        SendMessage(EXCHANGE_C2S, $"leave.{zone}", message);
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
            state = PlayerData.state,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"movement.{zone}", message);
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
    }

    public void SendPlayerTransfer(string targetZone)
    {
        var message = new
        {
            playerId = playerId,
            from = zone,
            to = targetZone,
            stanZdrowia = PlayerData.hp,
            cytryny = PlayerData.lemonsCount,
            igly = PlayerData.cactusNeedlesCount,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"transfer.{zone}", message);
        zone = targetZone; // Update local zone
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
                    else if (routingKey.StartsWith("interactions."))
                    {
                        ProcessInteractionMessage(message);
                    }
                    else if (routingKey.StartsWith("animations."))
                    {
                        ProcessAnimationMessage(message);
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
            channel.BasicConsume(interactionsQueue, true, consumer);
            channel.BasicConsume(animationsQueue, true, consumer);
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
        var data = JsonSerializer.Deserialize<MovementMessage>(message);
        if (data?.updates != null)
        {
            foreach (var update in data.updates)
            {
                if (update.id != playerId) // Update other players
                {
                    // Assuming Variables.cs has a way to manage other players' states
                    // You might need to implement a system to update other player GameObjects
                    Debug.Log($"Received movement for {update.id}: Position={update.position}, State={update.state}");
                }
            }
        }
    }

    void ProcessInteractionMessage(string message)
    {
        var data = JsonSerializer.Deserialize<InteractionMessage>(message);
        Debug.Log($"Interaction from {data.playerId}: {data.interaction}");
        // Handle interaction (e.g., update UI or trigger events)
    }

    void ProcessAnimationMessage(string message)
    {
        var data = JsonSerializer.Deserialize<AnimationMessage>(message);
        Debug.Log($"Animation from {data.playerId}: {data.animation}");
        // Handle animation (e.g., play animation on other player)
    }

    void ProcessTransferMessage(string message)
    {
        var data = JsonSerializer.Deserialize<TransferMessage>(message);
        if (data.playerId == playerId)
        {
            //playerStats.health = data.stanZdrowia;
            //playerStats.lemons = data.cytryny;
            //playerStats.needles = data.igly;
            zone = data.to;
            Debug.Log($"Player transferred to {zone}");
        }
    }

    void OnDestroy()
    {
        SendLeaveMessage();
        isRunning = false;
        channel?.Close();
        connection?.Close();
    }

    // Message classes for deserialization (adjust based on actual Variables.cs types)
    [Serializable]
    private class MovementMessage
    {
        public List<PlayerUpdate> updates;
    }

    [Serializable]
    private class PlayerUpdate
    {
        public string id;
        public Vector3 position;
        public Vector3 rotation;
        public string state;
        public float timestamp;
    }

    [Serializable]
    private class InteractionMessage
    {
        public string playerId;
        public string interaction;
        public float timestamp;
    }

    [Serializable]
    private class AnimationMessage
    {
        public string playerId;
        public string animation;
        public float timestamp;
    }

    [Serializable]
    private class TransferMessage
    {
        public string playerId;
        public string from;
        public string to;
        public float stanZdrowia;
        public int cytryny;
        public int igly;
        public float timestamp;
    }
}