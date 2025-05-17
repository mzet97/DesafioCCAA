using SistemaLivro.Domain.Domains.Books.Events.Books;
using SistemaLivro.Infrastructure.MessageBus;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SistemaLivro.Application.UseCases.Books.Notifications;

public class BookNotification(
    IMessageBusClient messageBusClient,
    ILogger<BookNotification> logger
    ) :
    INotificationHandler<BookCreated>,
    INotificationHandler<BookUpdated>,
    INotificationHandler<BookDisabled>,
    INotificationHandler<BookAtived>,
    INotificationHandler<BookDeleted>
{
    public async Task Handle(BookCreated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("BookCreated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "book.created",
             "book-exchange",
             "direct",
             "create-book");

        logger.LogInformation("BookCreated: " + notification);
    }

    public async Task Handle(BookUpdated notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("BookUpdated is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "book.updated",
             "book-exchange",
             "direct",
             "update-book");

        logger.LogInformation("BookUpdated: " + notification);
    }

    public async Task Handle(BookDisabled notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("BookDisabled is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "book.disabled",
             "book-exchange",
             "direct",
             "disable-book");

        logger.LogInformation("BookDisabled: " + notification);
    }

    public async Task Handle(BookAtived notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("BookAtived is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "book.atived",
             "book-exchange",
             "direct",
             "atived-book");

        logger.LogInformation("BookAtived: " + notification);
    }

    public async Task Handle(BookDeleted notification, CancellationToken cancellationToken)
    {
        if (notification is null)
        {
            logger.LogError("BookDeleted is null");
            return;
        }

        await messageBusClient.Publish(notification.ToString(),
             "book.deleted",
             "book-exchange",
             "direct",
             "deleted-book");

        logger.LogInformation("BookDeleted: " + notification);
    }
}
