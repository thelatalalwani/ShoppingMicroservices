using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Messaging;

public class RabbitMqPublisher
{
    public async Task PublishOrderCreatedAsync(object message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "host.docker.internal"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "order-created",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "order-created",
            body: body);
    }
}
