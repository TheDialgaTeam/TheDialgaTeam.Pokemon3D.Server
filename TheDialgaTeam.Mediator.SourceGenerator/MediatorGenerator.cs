using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheDialgaTeam.Mediator.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var getMediatorInterfaces = context.CompilationProvider.Select((compilation, token) => compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot(token).DescendantNodes())
            .Where(node => node is ClassDeclarationSyntax or RecordDeclarationSyntax)
            .Select(node => compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node, token))
            .Where(symbol => (symbol as ITypeSymbol)?.AllInterfaces.Any(typeSymbol => 
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Interfaces.IBaseRequest") || 
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Interfaces.INotification")) ?? false)
            .ToImmutableArray()
        );
        
        context.RegisterSourceOutput(getMediatorInterfaces, (productionContext, symbols) =>
        {
            var sourceBuilder = new SourceBuilder();

            sourceBuilder.WriteUsingNamespace(
                "global::Microsoft.Extensions.DependencyInjection", 
                "global::Microsoft.Extensions.DependencyInjection.Extensions", 
                "global::System.Collections.Concurrent",
                "global::System.Runtime.CompilerServices",
                "global::System.Threading", 
                "global::System.Threading.Tasks", 
                "global::TheDialgaTeam.Mediator.Interfaces");

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Extensions").WriteBlock(builder =>
            {
                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("public static class ServiceCollectionExtensions").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public static IServiceCollection AddMediator(this IServiceCollection collection)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("collection.TryAddSingleton<IMediator, TheDialgaTeam.Mediator.Implementations.Mediator>();");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(TheDialgaTeam.Mediator.Implementations.MediatorSender<>));");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(TheDialgaTeam.Mediator.Implementations.MediatorSender<,>));");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(TheDialgaTeam.Mediator.Implementations.MediatorPublisher<>));");
                        
                        methodBuilder.WriteLine("return collection;");
                    });
                });
            }).WriteEmptyLine();

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Interfaces").WriteBlock(builder =>
            {
                #region IMediatorSender
                
                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file interface IBaseMediatorSender").WriteBlock().WriteEmptyLine();

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file interface IMediatorSender : IBaseMediatorSender").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteGeneratedCodeAttribute();
                    interfaceBuilder.WriteLine("Task SendAsync(IRequest request, CancellationToken cancellationToken);");
                }).WriteEmptyLine();

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file interface IMediatorSender<TResponse> : IBaseMediatorSender").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteGeneratedCodeAttribute();
                    interfaceBuilder.WriteLine("Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken);");
                }).WriteEmptyLine();
                
                #endregion
                
                #region IMediatorPublisher
                
                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file interface IBaseMediatorPublisher").WriteBlock().WriteEmptyLine();

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file interface IMediatorPublisher : IBaseMediatorPublisher").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteGeneratedCodeAttribute();
                    interfaceBuilder.WriteLine("Task PublishAsync(INotification notification, CancellationToken cancellationToken);");
                }).WriteEmptyLine();
                
                #endregion
                
            }).WriteEmptyLine();

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Implementations").WriteBlock(builder =>
            {
                #region Mediator

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file sealed class Mediator : IMediator").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IServiceProvider _serviceProvider;");
                    classBuilder.WriteLine("private readonly ConcurrentDictionary<Type, IBaseMediatorSender> _senders = new();");
                    classBuilder.WriteLine("private readonly ConcurrentDictionary<Type, IBaseMediatorPublisher> _publishers = new();");
                    classBuilder.WriteLine("private readonly Dictionary<Type, Type> _mediatorTypes = new();");
                    
                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public Mediator(IServiceProvider serviceProvider)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_serviceProvider = serviceProvider;");

                        foreach (var symbol in symbols)
                        {
                            if (symbol is not ITypeSymbol typeSymbol) continue;

                            if (typeSymbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Interfaces.IBaseRequest")))
                            {
                                //constructorBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof(MediatorSender<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>));");
                            }
                            
                            if (typeSymbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Interfaces.INotification")))
                            {
                                constructorBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof(MediatorPublisher<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>));");
                            }
                        }
                    }).WriteEmptyLine();

                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorSender>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.SendAsync(request, cancellationToken);");
                    }).WriteEmptyLine();

                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorSender<TResponse>>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.SendAsync(request, cancellationToken);");
                    }).WriteEmptyLine();

                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorPublisher>(_publishers.GetOrAdd(notification.GetType(), static (type, args) => Unsafe.As<IBaseMediatorPublisher>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.PublishAsync(notification, cancellationToken);");
                    });
                });

                #endregion

                #region MediatorSender

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file sealed class MediatorSender<TRequest> : IMediatorSender where TRequest : IRequest").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IRequestHandler<TRequest> _handler;");
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPipeline<TRequest>> _pipelines;");

                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest> handler) : this(handler, Array.Empty<IRequestPipeline<TRequest>>())").WriteBlock().WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest> handler, IEnumerable<IRequestPipeline<TRequest>> pipelines)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handler = handler;");
                        constructorBuilder.WriteLine("_pipelines = pipelines;");
                    }).WriteEmptyLine();
                    
                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task SendAsync(IRequest request, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return _pipelines.Reverse().Aggregate(() => _handler.HandleAsync((TRequest) request, cancellationToken), (next, pipeline) => () => pipeline.HandleAsync((TRequest) request, next, cancellationToken))();");
                    });
                }).WriteEmptyLine();
                
                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file sealed class MediatorSender<TRequest, TResponse> : IMediatorSender<TResponse> where TRequest : IRequest<TResponse>").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IRequestHandler<TRequest, TResponse> _handler;");
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPipeline<TRequest, TResponse>> _pipelines;");

                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest, TResponse> handler) : this(handler, Array.Empty<IRequestPipeline<TRequest, TResponse>>())").WriteBlock().WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IRequestPipeline<TRequest, TResponse>> pipelines)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handler = handler;");
                        constructorBuilder.WriteLine("_pipelines = pipelines;");
                    }).WriteEmptyLine();
                    
                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return _pipelines.Reverse().Aggregate(() => _handler.HandleAsync((TRequest) request, cancellationToken), (next, pipeline) => () => pipeline.HandleAsync((TRequest) request, next, cancellationToken))();");
                    });
                }).WriteEmptyLine();

                #endregion

                #region MediatorPublisher

                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("file sealed class MediatorPublisher<TNotification> : IMediatorPublisher where TNotification : INotification").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<INotificationHandler<TNotification>> _handlers;");
                    
                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorPublisher() : this(Array.Empty<INotificationHandler<TNotification>>())").WriteBlock().WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorPublisher(IEnumerable<INotificationHandler<TNotification>> handlers)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handlers = handlers;");
                    }).WriteEmptyLine();
                    
                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("public Task PublishAsync(INotification notification, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return Task.WhenAll(_handlers.Select(handler => handler.HandleAsync((TNotification) notification, cancellationToken)));");
                    });
                }).WriteEmptyLine();

                #endregion
            });

            productionContext.AddSource("Mediator.cs", sourceBuilder.ToString());
        });
    }
}