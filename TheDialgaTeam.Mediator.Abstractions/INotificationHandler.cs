namespace TheDialgaTeam.Mediator.Abstractions;

public interface INotificationHandler
{
}

public interface INotificationHandler<in TNotification> : INotificationHandler where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
}