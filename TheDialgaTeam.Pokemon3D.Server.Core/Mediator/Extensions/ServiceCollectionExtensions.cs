// Pokemon 3D Server Client
// Copyright (C) 2023 Yong Jian Ming
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations.Middlewares;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection collection)
    {
        collection.AddSingleton<IMediator, Implementations.Mediator>();
        collection.AddSingleton(typeof(MediatorSender<>));
        collection.AddSingleton(typeof(MediatorSender<,>));
        collection.AddSingleton(typeof(MediatorPublisher<>));

        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>), typeof(PostProcessorRequestMiddleware<>)));
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>), typeof(PostProcessorRequestMiddleware<,>)));

        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>), typeof(PreProcessorRequestMiddleware<>)));
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>), typeof(PreProcessorRequestMiddleware<,>)));

        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>), typeof(LogSlowRequestMiddleware<>)));
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>), typeof(LogSlowRequestMiddleware<,>)));
        
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>), typeof(CatchUnhandledExceptionMiddleware<>)));
        collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>), typeof(CatchUnhandledExceptionMiddleware<,>)));
        
        return collection;
    }

    [RequiresUnreferencedCode("TRequestHandler\'s dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddRequestHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] TRequestHandler>(this IServiceCollection collection, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TRequestHandler : IRequestHandler
    {
        collection.TryAdd(ServiceDescriptor.Describe(typeof(TRequestHandler), typeof(TRequestHandler), serviceLifetime));
        
        foreach (var @interface in GetInterfacesRecursively(typeof(TRequestHandler)))
        {
            if (!@interface.IsGenericType) continue;
            
            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>) || @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            {
                collection.TryAdd(ServiceDescriptor.Describe(@interface, static provider => provider.GetRequiredService(typeof(TRequestHandler)), serviceLifetime));
            }
        }

        return collection;
    }

    [RequiresUnreferencedCode("TNotificationHandler\'s dependent types may have their members trimmed. Ensure all required members are preserved.")]
    public static IServiceCollection AddNotificationHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] TNotificationHandler>(this IServiceCollection collection, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TNotificationHandler : INotificationHandler
    {
        collection.TryAdd(ServiceDescriptor.Describe(typeof(TNotificationHandler), typeof(TNotificationHandler), serviceLifetime));

        foreach (var @interface in GetInterfacesRecursively(typeof(TNotificationHandler)))
        {
            if (!@interface.IsGenericType) continue;
            
            if (@interface.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
            {
                collection.TryAddEnumerable(ServiceDescriptor.Describe(@interface, static provider => provider.GetRequiredService(typeof(TNotificationHandler)), serviceLifetime));
            }
        }
        
        return collection;
    }

    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    private static IEnumerable<Type> GetInterfacesRecursively([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type)
    {
        var result = new List<Type>();
        CollectInterfacesRecursively(type, result);
        return result;
    }
    
    [RequiresUnreferencedCode("Dependent types may have their members trimmed. Ensure all required members are preserved.")]
    private static void CollectInterfacesRecursively([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type, ICollection<Type> interfaces)
    {
        foreach (var @interface in type.GetInterfaces())
        {
            if (interfaces.Contains(@interface)) continue;
            interfaces.Add(@interface);
            CollectInterfacesRecursively(@interface, interfaces);
        }
    }
}