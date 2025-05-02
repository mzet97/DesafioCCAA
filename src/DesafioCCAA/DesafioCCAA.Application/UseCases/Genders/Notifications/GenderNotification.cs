using DesafioCCAA.Domain.Domains.Books.Events.Genders;
using DesafioCCAA.Infrastructure.MessageBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DesafioCCAA.Application.UseCases.Genders.Notifications;

public class GenderNotification(
    IMessageBusClient messageBusClient,
    ILogger<GenderNotification> logger
    ) :
    INotificationHandler<GenderCreated>,
    INotificationHandler<GenderUpdated>,
    INotificationHandler<GenderDisabled>,
    INotificationHandler<GenderAtived>,
    INotificationHandler<GenderDeleted>
{
    public async Task Handle(GenderCreated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("GenderCreated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "gender.created",
             "gender-exchange",
             "direct",
             "create-gender");

        logger.LogInformation("GenderCreated: " + notification);
    }

    public async Task Handle(GenderUpdated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("GenderUpdated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "gender.updated",
             "gender-exchange",
             "direct",
             "update-gender");

        logger.LogInformation("GenderUpdated: " + notification);
    }

    public async Task Handle(GenderDisabled notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("GenderDisabled is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "gender.disabled",
             "gender-exchange",
             "direct",
             "disable-gender");

        logger.LogInformation("GenderDisabled: " + notification);
    }

    public async Task Handle(GenderAtived notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("GenderAtived is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "gender.atived",
             "gender-exchange",
             "direct",
             "atived-gender");

        logger.LogInformation("GenderAtived: " + notification);
    }

    public async Task Handle(GenderDeleted notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("GenderDeleted is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "gender.deleted",
             "gender-exchange",
             "direct",
             "deleted-gender");

        logger.LogInformation("GenderDeleted: " + notification);
    }
}
