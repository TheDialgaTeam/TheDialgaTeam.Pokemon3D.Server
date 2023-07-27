namespace TheDialgaTeam.Mediator.Abstractions;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}