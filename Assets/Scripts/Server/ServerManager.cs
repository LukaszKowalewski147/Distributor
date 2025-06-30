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
    public static ServerManager Instance { get; private set; }

    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private readonly object lockObject = new object();

    private IConnection connection;
    private IModel channel;
    private Thread consumerThread;
    private volatile bool isRunning;

    private string myPlayerID;
    private string myPlayerName;
    
    // RabbitMQ configuration (matching server)
    private const string RABBITMQ_HOST = "localhost";
    private const string EXCHANGE_C2S = "game.client_to_server";
    private const string EXCHANGE_M2C = "game.movement_to_client";
    private const string EXCHANGE_A2C = "game.animations_to_client";
    private const string EXCHANGE_I2C = "game.interactions_to_client";
    //private const string EXCHANGE_PT = "game.player_transfer";
    private const float TICK_INTERVAL = 0.25f; // 250ms, matching server

    // Queues and routing keys
    private string movementQueue;
    private string animationsQueue;
    private string interactionsQueue;
    private string transferQueue;

    void Awake()
    {
        if (!PlayerData.multiplayer)
        {
            Destroy(gameObject);
            return;
        }

        #region Singleton handling
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        #endregion
    }

    void Start()
    {
        Debug.Log("ServerManager.cs - Start()");
        // Initialize otherPlayers, myPlayerID, myPlayerName and myPlayerZone
        myPlayerID = PlayerData.playerID;
        myPlayerName = PlayerData.playerName;

        // Initialize queue names
        movementQueue = $"movement.{myPlayerID}";
        animationsQueue = $"animations.{myPlayerID}";
        interactionsQueue = $"interactions.{myPlayerID}";
        transferQueue = $"transfer.{PlayerData.zone}";

        ConnectToRabbitMQ();
        SendJoinMessage();
        StartSendingUpdates();
        StartConsuming();
    }

    void Update()
    {
        lock (lockObject)
        {
            while (mainThreadActions.Count > 0)
            {
                Action action = mainThreadActions.Dequeue();
                action?.Invoke();
            }
        }
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
            //channel.ExchangeDeclare(EXCHANGE_PT, ExchangeType.Topic);

            // Declare and bind client-specific queues
            channel.QueueDeclare(movementQueue, false, false, true, null);
            channel.QueueBind(movementQueue, EXCHANGE_M2C, movementQueue);

            channel.QueueDeclare(animationsQueue, false, false, true, null);
            channel.QueueBind(animationsQueue, EXCHANGE_A2C, animationsQueue);

            channel.QueueDeclare(interactionsQueue, false, false, true, null);
            channel.QueueBind(interactionsQueue, EXCHANGE_I2C, interactionsQueue);

            channel.QueueDeclare(transferQueue, false, false, true, null);
            channel.QueueBind(transferQueue, EXCHANGE_C2S, transferQueue);

            Debug.Log("ServerManager.cs - ConnectToRabbitMQ(): Connected to RabbitMQ and queues set up.");
        }
        catch (Exception e)
        {
            Debug.LogError($"ServerManager.cs - ConnectToRabbitMQ(): Failed to connect to RabbitMQ: {e.Message}");
        }
    }

    void SendJoinMessage()
    {
        Debug.Log("ServerManager.cs - SendJoinMessage()");
        var message = new { id = myPlayerID };
        SendMessage(EXCHANGE_C2S, $"join.{PlayerData.zone}", message);
        //Debug.Log("ServerManager.cs - SendLeaveMessage(): msg: " + message.ToString());
    }

    void SendLeaveMessage()
    {
        Debug.Log("ServerManager.cs - SendLeaveMessage()");
        var message = new { id = myPlayerID };
        SendMessage(EXCHANGE_C2S, $"leave.{PlayerData.zone}", message);
        //Debug.Log("ServerManager.cs - SendLeaveMessage(): msg: " + message.ToString());
    }

    void SendMessage(string exchange, string routingKey, object message)
    {
        //Debug.Log("ServerManager.cs - SendMessage(): rutngKey: " + routingKey + " | exchng: " + exchange + "\nmessage: " + message.ToString());
        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish(exchange, routingKey, null, body);
            //Debug.Log("send message: after basic publish");
        }
        catch (Exception e)
        {
            Debug.LogError($"ServerManager.cs - SendMessage(): Error sending message to {routingKey}: {e.Message}");
        }
    }

    void StartSendingUpdates()
    {
        Debug.Log("ServerManager.cs - StartSendingUpdates()");
        InvokeRepeating(nameof(SendPlayerUpdate), 0f, TICK_INTERVAL);
    }

    void SendPlayerUpdate()
    {
        //Debug.Log("send player update");
        Vector3 position = PlayerData.position;

        var message = new
        {
            id = myPlayerID,
            posX = position.x,
            posY = position.y,
            posZ = position.z,
            rotY = PlayerData.rotationY,
            timestamp = Time.time
        };
        //Debug.Log("player update: message: " + message.ToString());
        SendMessage(EXCHANGE_C2S, $"movement.{PlayerData.zone}", message);
        //Debug.Log("SERVER - Sending a MOVEMENT UPDATE MESSAGE:\n" + message.ToString());
    }

    public void SendAnimation(string animation)
    {
        var message = new
        {
            playerId = myPlayerID,
            animation,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"animations.{PlayerData.zone}", message);
        //Debug.Log("SERVER - Sending a ANIMATION TRIGGER MESSAGE:\n" + message.ToString());
    }
    
    public void SendInteraction(string interaction)
    {
        var message = new
        {
            playerId = myPlayerID,
            interaction,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"interactions.{PlayerData.zone}", message);
        //Debug.Log("SERVER - Sending a INTERACTION TRIGGER MESSAGE:\n" + message.ToString());
    }
    
    public void SendPlayerTransfer(string targetZone)
    {
        Debug.Log("ServerManager.cs - SendPlayerTransfer()");
        var message = new
        {
            playerId = myPlayerID,
            from = PlayerData.zone,
            to = targetZone,
            timestamp = Time.time
        };
        SendMessage(EXCHANGE_C2S, $"transfer.{PlayerData.zone}", message);
        //Debug.Log("ServerManager.cs - SendPlayerTransfer(): msg: " + message.ToString());

        PlayerData.zone = targetZone; // Update local zone

        //UpdateQueueBindings();
    }
    
    void UpdateQueueBindings()
    {
        // Unbind and delete old transfer queue
        //channel.QueueUnbind(transferQueue, EXCHANGE_PT, transferQueue, null);
        //channel.QueueDelete(transferQueue);

        // Update transfer queue for new zone
        //transferQueue = $"transfer.*.{PlayerData.zone}";
        //channel.QueueDeclare(transferQueue, false, false, true, null);
        //channel.QueueBind(transferQueue, EXCHANGE_PT, transferQueue);
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
                    //else if (routingKey.StartsWith("transfer."))
                    //{
                    //    ProcessTransferMessage(message);
                    //}
                }
                catch (Exception e)
                {
                    Debug.LogError($"ServerManager.cs - ConsumeMessages(): Error processing message: {e.Message}");
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
            Debug.LogError($"ServerManager.cs - ConsumeMessages(): Consumer thread error: {e.Message}");
        }
    }

    void ProcessMovementMessage(string message)
    {
        //Debug.Log("SERVER - movement response message: " + message);
        var data = JsonSerializer.Deserialize<MessageClasses.MovementMessage>(message);

        if (data?.Updates != null)
        {
            lock (lockObject)
            {
                mainThreadActions.Enqueue(() => PlayerData.otherPlayers.ManageServerMovementUpdates(data.Updates));
            }
        }
    }

    void ProcessAnimationMessage(string message)
    {
        var data = JsonSerializer.Deserialize<MessageClasses.AnimationMessage>(message);

        if (data != null)
        {
            if (data.PlayerId != myPlayerID)
            {
                lock (lockObject)
                {
                    mainThreadActions.Enqueue(() => PlayerData.otherPlayers.ManageServerAnimationTrigger(data));
                }
            }
        }
        //Debug.Log($"Animation from {data.PlayerId}: animation: {data.Animation}");
        // Handle animation (e.g., play animation on other player)
    }

    void ProcessInteractionMessage(string message)
    {
        var data = JsonSerializer.Deserialize<MessageClasses.InteractionMessage>(message);
        string playerID = data.PlayerId;

        Debug.LogWarning("ServerManager.cs - ProcessInteractionMessage(): " + playerID + ": " + data.Interaction);

        if (playerID != myPlayerID)
        {
            Debug.LogWarning("ServerManager.cs - ProcessInteractionMessage(): ID: " + playerID + " is not mine");
            if (data.Interaction == "left")
            {
                Debug.LogWarning("ServerManager.cs - ProcessInteractionMessage(): " + playerID + " left the server");
                lock (lockObject)
                {
                    mainThreadActions.Enqueue(() => PlayerData.otherPlayers.RemovePlayer(playerID));
                }
            }
        }
    }
    /*
    void ProcessTransferMessage(string message)
    {
        Debug.LogWarning("ServerManager.cs - ProcessTransferMessage()");
        var data = JsonSerializer.Deserialize<MessageClasses.TransferMessage>(message);
        string playerID = data.PlayerId;

        if (playerID != myPlayerID)
        {
            lock (lockObject)
            {
                mainThreadActions.Enqueue(() => PlayerData.otherPlayers.RemovePlayer(playerID));
            }
        }

        Debug.Log("ServerManager.cs - ProcessTransferMessage(): Player " + playerID + " transferred to: " + data.To);
    }
    */
    private void OnApplicationQuit()
    {
        Debug.LogWarning("ServerManager.cs - OnApplicationQuit(): Closing RabbitMQ connection...");
        try
        {
            SendLeaveMessage();
            isRunning = false; 

            if (consumerThread != null && consumerThread.IsAlive)
            {
                consumerThread.Join(5000);
            }

            if (channel != null && channel.IsOpen)
            {
                try
                {
                    channel.Close();
                    Debug.Log("ServerManager.cs - OnApplicationQuit(): Channel closed successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"ServerManager.cs - OnApplicationQuit(): Error closing channel: {e.Message}");
                }
            }

            if (connection != null && connection.IsOpen)
            {
                try
                {
                    connection.Close();
                    Debug.Log("ServerManager.cs - OnApplicationQuit(): Connection closed successfully.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"ServerManager.cs - OnApplicationQuit(): Error closing connection: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ServerManager.cs - OnApplicationQuit(): Unexpected error: {e.Message}");
        }
    }
}