using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory { HostName = "localhost" };

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    var (type, exchangeName)= SetupBusChannel(channel);

    /*
     1 - fast-queue
     2 - normal-queue
     3 - preferential-queue
    */

    var groceryQueues = new List<string>() { "fast", "normal", "preferential" };
    var random = new Random();

    if (type.Equals(ExchangeType.Topic)) groceryQueues.Add("normal.busy");

    while (true)
    {
        Console.WriteLine("Press any key to generate more messages");
        Console.ReadLine();

        var routingKey = groceryQueues[random.Next(0, groceryQueues.Count)];

        var message = $"This Consumer has a message for queue {routingKey}";

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: exchangeName,
                             routingKey: routingKey,
                             basicProperties: null,
                             body: body);

        Console.WriteLine($" [x] Sent message for queue {routingKey}");
    }
}

(string, string) SetupBusChannel(IModel channel)
{
    channel.QueueDeclare(queue: "fast-queue", durable: true, exclusive: false, autoDelete: false);
    channel.QueueDeclare(queue: "normal-queue", durable: true, exclusive: false, autoDelete: false);
    channel.QueueDeclare(queue: "preferential-queue", durable: true, exclusive: false, autoDelete: false);

    // On a busy day, for example, people who usually don't go to the fast queue have access to it
    // This situation is for the sake of using the topic exchange
    var usedExchangeType = ExchangeType.Topic;
    var usedExchangeName = "grocery";

    channel.ExchangeDeclare(usedExchangeName, usedExchangeType);

    channel.QueueBind("fast-queue", usedExchangeName, "fast");
    channel.QueueBind("normal-queue", usedExchangeName, "normal");
    channel.QueueBind("preferential-queue", usedExchangeName, "preferential");

    if (usedExchangeType.Equals(ExchangeType.Topic)) channel.QueueBind("fast-queue", usedExchangeName, "*.busy");

    return (usedExchangeType, usedExchangeName);
}
