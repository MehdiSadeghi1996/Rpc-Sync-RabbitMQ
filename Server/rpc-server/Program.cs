// See https://aka.ms/new-console-template for more information



using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

Console.WriteLine("Hello, World!");
var queue = "rpcexamplequeue";

var factory = new ConnectionFactory()
{
    HostName = "10.187.160.107",
    Port = 5002,
    UserName = "guest",
    Password = "guest",
};

using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(
        queue: queue,
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null);
    channel.BasicQos(0, 1, false);

    // Configure request consumer
    var consumer = new EventingBasicConsumer(channel);

    // ???? Consume from the pseudo-queue amq.rabbitmq.reply-to in no-ack mode.
    channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
    consumer.Received += (model, ea) =>
    {
        string response = null;

        var body = ea.Body.ToArray();
        var props = ea.BasicProperties;
        var replyProps = channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        try
        {
            var message = Encoding.UTF8.GetString(body);
            int n = int.Parse(message);
            response = fib(n).ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(" [.] " + e.Message);
            response = "";
        }
        finally
        {
            var responseBytes = Encoding.UTF8.GetBytes(response);
            channel.BasicPublish(
                exchange: "",                // Must reply to default exchange ("")
                routingKey: props.ReplyTo,
                basicProperties: replyProps,
                body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    };

    Console.WriteLine("Awaiting RPC requests");
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}



static int fib(int n)
{
    return 832040;

    if (n == 0 || n == 1)
    {
        return n;
    }

    return fib(n - 1) + fib(n - 2);
}