using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Implementations;
using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Implementations.Middleware;
using TheDialgaTeam.Pokemon3D.Server.Core.Queries.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Queries.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection collection)
    {
        collection.AddSingleton(typeof(IQueryMiddleware<,>), typeof(UnhandledExceptionQueryMiddleware<,>));
        collection.AddSingleton(typeof(IQueryMiddleware<,>), typeof(LogSlowQueryMiddleware<,>));
        
        collection.AddSingleton(typeof(QueryDispatcherHelper<,>));
        collection.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        
        return collection;
    }

    public static IServiceCollection AddQueryHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicConstructors)] TQueryHandler>(this IServiceCollection collection, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TQueryHandler : IQueryHandler
    {
        collection.TryAdd(ServiceDescriptor.Describe(typeof(TQueryHandler), typeof(TQueryHandler), serviceLifetime));

        foreach (var @interface in typeof(TQueryHandler).GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
            {
                collection.TryAdd(ServiceDescriptor.Describe(@interface, static provider => provider.GetRequiredService(typeof(TQueryHandler)), serviceLifetime));
            }
        }
        
        return collection;
    }
}