using SistemaLivro.Domain.Domains.Books.Events.Publishers;
using SistemaLivro.Infrastructure.MessageBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SistemaLivro.Application.UseCases.Publishers.Notifications;

public class PublisherNotification(
    IMessageBusClient messageBusClient,
    ILogger<PublisherNotification> logger
    ) :
    INotificationHandler<PublisherCreated>,
    INotificationHandler<PublisherUpdated>,
    INotificationHandler<PublisherDisabled>,
    INotificationHandler<PublisherAtived>,
    INotificationHandler<PublisherDeleted>
{
    public async Task Handle(PublisherCreated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("PublisherCreated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "publisher.created",
             "publisher-exchange",
             "direct",
             "create-publisher");

        logger.LogInformation("PublisherCreated: " + notification);
    }

    public async Task Handle(PublisherUpdated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("PublisherUpdated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "publisher.updated",
             "publisher-exchange",
             "direct",
             "update-publisher");

        logger.LogInformation("PublisherUpdated: " + notification);
    }

    public async Task Handle(PublisherDisabled notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("PublisherDisabled is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "publisher.disabled",
             "publisher-exchange",
             "direct",
             "disable-publisher");

        logger.LogInformation("PublisherDisabled: " + notification);
    }

    public async Task Handle(PublisherAtived notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("PublisherAtived is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "publisher.atived",
             "publisher-exchange",
             "direct",
             "atived-publisher");

        logger.LogInformation("PublisherAtived: " + notification);
    }

    public async Task Handle(PublisherDeleted notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("PublisherDeleted is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "publisher.deleted",
             "publisher-exchange",
             "direct",
             "deleted-publisher");

        logger.LogInformation("PublisherDeleted: " + notification);
    }
}
