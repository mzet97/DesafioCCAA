using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesafioCCAA.Infrastructure.MessageBus;

public interface IMessageBusClient
{
    Task Publish(
        object message,
        string routingKey,
        string exchange,
        string type,
        string queueName);

    Task Subscribe(
        string queueName,
        string exchange,
        string type,
        string routingKey,
        Action<string> onMessageReceived);
}

