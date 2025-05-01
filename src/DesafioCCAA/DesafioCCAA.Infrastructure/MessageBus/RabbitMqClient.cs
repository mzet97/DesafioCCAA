using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DesafioCCAA.Infrastructure.MessageBus;

public class RabbitMqClient : IMessageBusClient
{
    private readonly IConnection _connection;

    public RabbitMqClient(ProducerConnection producerConnection)
    {
        _connection = producerConnection.Connection;
    }

    public async Task Publish(
        object message,
        string routingKey,
        string exchange,
        string type,
        string queueName)
    {
        var channel = await _connection.CreateChannelAsync();

        JsonSerializerOptions settings = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var payload = JsonSerializer.Serialize(message, settings);
        var body = Encoding.UTF8.GetBytes(payload);

        await channel.ExchangeDeclareAsync(exchange, type, durable: true);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queueName, exchange, routingKey);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
             exchange: exchange,
             routingKey: routingKey,
             mandatory: false,
             basicProperties: properties,
             body: body);
    }

    public async Task Subscribe(
        string queueName,
        string exchange,
        string type,
        string routingKey,
        Action<string> onMessageReceived)
    {
        var channel = await _connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange, type, durable: true);
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueBindAsync(queueName, exchange, routingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            onMessageReceived(message);
            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, autoAck: true, consumer);
    }
}
