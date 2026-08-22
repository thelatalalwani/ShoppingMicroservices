using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Messaging;

public class OrderCreatedConsumer : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: "order-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-queue-type"] = "classic"
            });

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Order notification received: {message}");

                await _channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                await _channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "order-created",
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
