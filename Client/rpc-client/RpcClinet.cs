using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace rpc_client
{
    public class RpcClinet
    {

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;
        public RpcClinet(string queueName)
        {
            this.queueName = queueName;

            var factory = new ConnectionFactory()
            {
                HostName = "10.187.160.107",
                Port = 5002,
                UserName = "guest",
                Password = "guest",
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = "amq.rabbitmq.reply-to";

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }
        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            // The RPC client must consume in the automatic acknowledgement mode.
            channel.BasicConsume(
                consumer: consumer,
                queue: "amq.rabbitmq.reply-to",
                autoAck: true);

            channel.BasicPublish(
                exchange: "",
                routingKey: this.queueName,
                basicProperties: props,
                body: messageBytes);

            return respQueue.Take();

        }
        public void Close()
        {
            connection.Close();
        }

    }
}
