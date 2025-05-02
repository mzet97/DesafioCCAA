using DesafioCCAA.Infrastructure.MessageBus;
using DesafioCCAA.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace DesafioCCAA.Infrastructure.Configuration;

public static class MessageBusConfig
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionFactory = new ConnectionFactory();

        var rabbitMqSection = configuration.GetSection("RabbitMq");
        services.Configure<RabbitMq>(rabbitMqSection);
        var rabbitMq = rabbitMqSection.Get<RabbitMq>();

        if (rabbitMq == null)
        {
            throw new ArgumentNullException("RabbitMq configuration is missing");
        }

        connectionFactory.HostName = rabbitMq.HostName;
        connectionFactory.Port = rabbitMq.Port;
        connectionFactory.UserName = rabbitMq.Username;
        connectionFactory.Password = rabbitMq.Password;

        services.AddSingleton(async serviceProvider =>
        {
            var connection = await connectionFactory.CreateConnectionAsync(rabbitMq.ClientProviderName);
            return connection;
        });

        services.AddSingleton(serviceProvider =>
        {
            var connectionTask = serviceProvider.GetRequiredService<Task<IConnection>>();
            var connection = connectionTask.GetAwaiter().GetResult();
            return new ProducerConnection(connection);
        });

        services.AddSingleton<IMessageBusClient>(serviceProvider =>
        {
            var producerConnection = serviceProvider.GetRequiredService<ProducerConnection>();
            return new RabbitMqClient(producerConnection);
        });

        return services;
    }
}
