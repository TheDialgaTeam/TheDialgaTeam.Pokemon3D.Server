namespace TheDialgaTeam.Mediator.Interfaces;

public interface INotificationHandler
{
}

public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
}