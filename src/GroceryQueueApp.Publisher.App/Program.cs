using RabbitMQ.Client;
using System.Runtime.CompilerServices;
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

    var groceryQueues = new string[] { "fast-queue", "normal-queue", "preferential-queue" };
    var random = new Random();

    while (true)
    {
        Console.WriteLine("Press any key to generate more messages");
        Console.ReadLine();


    }


    var message = "fast-queue";

    var body = Encoding.UTF8.GetBytes(message);

    channel.BasicPublish(exchange: "grocery",
                         routingKey: string.Empty,
                         basicProperties: null,
                         body: body);

    Console.WriteLine($" [x] Sent {message}");

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}

IModel SetupBusChannel(IModel channel)
{

    {
        channel.QueueDeclare(queue: "fast-queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: "normal-queue", durable: false, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: "preferential-queue", durable: false, exclusive: false, autoDelete: false);

        channel.ExchangeDeclare("grocery", "direct");

        return channel;
    }
}
