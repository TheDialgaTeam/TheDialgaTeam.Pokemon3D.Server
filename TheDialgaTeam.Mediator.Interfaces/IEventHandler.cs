namespace TheDialgaTeam.Mediator.Interfaces;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}