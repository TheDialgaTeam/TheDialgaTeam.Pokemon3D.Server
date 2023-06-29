using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces;
using TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Middlewares;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection collection)
    {
        collection.AddSingleton<IMediator, Mediator>();
        collection.AddSingleton(typeof(MediatorSender<>));
        collection.AddSingleton(typeof(MediatorSender<,>));
        
        return collection;
    }
    
    public static IServiceCollection AddRequestHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TRequestHandler>(this IServiceCollection collection, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TRequestHandler : IRequestHandler
    {
        collection.TryAdd(ServiceDescriptor.Describe(typeof(TRequestHandler), typeof(TRequestHandler), serviceLifetime));

        foreach (var @interface in typeof(TRequestHandler).GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>) || @interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            {
                collection.TryAdd(ServiceDescriptor.Describe(@interface, static provider => provider.GetRequiredService(typeof(TRequestHandler)), serviceLifetime));
            }
        }
        return collection;
    }
    
    public static IServiceCollection AddRequestHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TRequestHandler>(this IServiceCollection collection, Action<RequestHandlerBuilder<TRequestHandler>> configure, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TRequestHandler : IRequestHandler
    {
        configure(new RequestHandlerBuilder<TRequestHandler>(collection));
        return AddRequestHandler<TRequestHandler>(collection, serviceLifetime);
    }
}

public sealed class RequestHandlerBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TRequestHandler> where TRequestHandler : IRequestHandler
{
    private readonly IServiceCollection _collection;

    public RequestHandlerBuilder(IServiceCollection collection)
    {
        _collection = collection;
    }
    
    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can\'t validate that the requirements of those annotations are met.")]
    public RequestHandlerBuilder<TRequestHandler> UseLogSlowRequestMiddleware()
    {
        foreach (var @interface in typeof(TRequestHandler).GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
            {
                var types = @interface.GetGenericArguments();
                _collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>).MakeGenericType(types), typeof(LogSlowRequestMiddleware<>).MakeGenericType(types)));
            }

            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            {
                var types = @interface.GetGenericArguments();
                _collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>).MakeGenericType(types), typeof(LogSlowRequestMiddleware<,>).MakeGenericType(types)));
            }
        }
        
        return this;
    }
    
    [RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
    [RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can\'t validate that the requirements of those annotations are met.")]
    public RequestHandlerBuilder<TRequestHandler> UseCatchUnhandledExceptionMiddleware()
    {
        foreach (var @interface in typeof(TRequestHandler).GetInterfaces())
        {
            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<>))
            {
                var types = @interface.GetGenericArguments();
                _collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<>).MakeGenericType(types), typeof(CatchUnhandledExceptionMiddleware<>).MakeGenericType(types)));
            }

            if (@interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            {
                var types = @interface.GetGenericArguments();
                _collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestMiddleware<,>).MakeGenericType(types), typeof(CatchUnhandledExceptionMiddleware<,>).MakeGenericType(types)));
            }
        }
        
        return this;
    }
}