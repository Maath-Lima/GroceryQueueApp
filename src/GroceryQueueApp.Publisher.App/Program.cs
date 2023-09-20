using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    SetupBusChannel(channel);

    /*
     1 - fast-queue
     2 - normal-queue
     3 - preferential-queue
    */

    var groceryQueues = new string[] { "fast", "normal", "preferential" };
    var random = new Random();

    while (true)
    {
        Console.WriteLine("Press any key to generate more messages");
        Console.ReadLine();

        var routingKey = groceryQueues[random.Next(0, groceryQueues.Length)];

        var message = $"This Consumer has a message for queue {routingKey}";

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "grocery",
                             routingKey: routingKey,
                             basicProperties: null,
                             body: body);

        Console.WriteLine($" [x] Sent message for queue {routingKey}");
    }
}

IModel SetupBusChannel(IModel channel)
{
    channel.QueueDeclare(queue: "fast-queue", durable: true, exclusive: false, autoDelete: false);
    channel.QueueDeclare(queue: "normal-queue", durable: false, exclusive: false, autoDelete: false);
    channel.QueueDeclare(queue: "preferential-queue", durable: false, exclusive: false, autoDelete: false);

    channel.ExchangeDeclare("grocery", ExchangeType.Direct);

    channel.QueueBind("fast-queue", "grocery", "fast");
    channel.QueueBind("normal-queue", "grocery", "normal");
    channel.QueueBind("preferential-queue", "grocery", "preferential");

    return channel;
}
