using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    SetupAndRunConsumer(channel, $"Consumer F1", "fast-queue");
    SetupAndRunConsumer(channel, $"Consumer N1", "normal-queue");
    SetupAndRunConsumer(channel, $"Consumer P1", "preferential-queue");

    Console.ReadLine();
}

void SetupAndRunConsumer(IModel channel, string consumerName, string queueName)
{
    var consumer = new EventingBasicConsumer(channel);

    consumer.Received += (model, ea) =>
    {
        var body = ea.Body;
        var message = Encoding.UTF8.GetString(body.ToArray());

        Console.WriteLine($"{consumer}: [x] message: {message}");

        //channel.BasicAck(ea.DeliveryTag, false);
    };

    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
}