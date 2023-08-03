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
            .Where(symbol => symbol != null && ((ITypeSymbol) symbol).AllInterfaces.Any(typeSymbol => 
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Abstractions.IBaseRequest") || 
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Abstractions.INotification") ||
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Abstractions.INotificationHandler")))
            .OfType<ITypeSymbol>()
            .ToImmutableArray()
        );
        
        context.RegisterSourceOutput(getMediatorInterfaces, (productionContext, symbols) =>
        {
            var sourceBuilder = new SourceBuilder();

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Extensions").WriteBlock(builder =>
            {
                builder.WriteUsingNamespace(
                    "global::Microsoft.Extensions.DependencyInjection", 
                    "global::Microsoft.Extensions.DependencyInjection.Extensions",
                    "global::TheDialgaTeam.Mediator.Abstractions",
                    "global::TheDialgaTeam.Mediator.Implementations",
                    "global::TheDialgaTeam.Mediator.Implementations.Pipelines");
                
                builder.WriteLine("public static class ServiceCollectionExtensions").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("public static IServiceCollection AddMediator(this IServiceCollection collection)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("collection.TryAddSingleton<IMediator, Implementations.Mediator>();");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(MediatorSender<>));");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(MediatorSender<,>));");
                        methodBuilder.WriteLine("collection.TryAddSingleton(typeof(MediatorPublisher<>));");
                        
                        methodBuilder.WriteLine("collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestPipeline<>), typeof(PreProcessorRequestPipeline<>)));");
                        methodBuilder.WriteLine("collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestPipeline<,>), typeof(PreProcessorRequestPipeline<,>)));");
                        methodBuilder.WriteLine("collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestPipeline<>), typeof(PostProcessorRequestPipeline<>)));");
                        methodBuilder.WriteLine("collection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IRequestPipeline<,>), typeof(PostProcessorRequestPipeline<,>)));");

                        methodBuilder.WriteLine("return collection;");
                    });
                });
            });

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Abstractions").WriteBlock(builder =>
            {
                builder.WriteUsingNamespace(
                    "global::System.Threading",
                    "global::System.Threading.Tasks",
                    "global::TheDialgaTeam.Mediator.Abstractions");
                
                #region IMediatorSender
                
                builder.WriteLine("file interface IBaseMediatorSender").WriteBlock();
                
                builder.WriteLine("file interface IMediatorSender : IBaseMediatorSender").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteLine("Task SendAsync(IRequest request, CancellationToken cancellationToken);");
                });
                
                builder.WriteLine("file interface IMediatorSender<TResponse> : IBaseMediatorSender").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteLine("Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken);");
                });
                
                #endregion
                
                #region IMediatorPublisher
                
                builder.WriteLine("file interface IBaseMediatorPublisher").WriteBlock();
                
                builder.WriteLine("file interface IMediatorPublisher : IBaseMediatorPublisher").WriteBlock(interfaceBuilder =>
                {
                    interfaceBuilder.WriteLine("Task PublishAsync(INotification notification, CancellationToken cancellationToken);");
                });
                
                #endregion
                
            });

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Implementations").WriteBlock(builder =>
            {
                builder.WriteUsingNamespace(
                    "global::Microsoft.Extensions.DependencyInjection", 
                    "global::Microsoft.Extensions.DependencyInjection.Extensions",
                    "global::System.Collections.Concurrent",
                    "global::System.Runtime.CompilerServices",
                    "global::System.Threading", 
                    "global::System.Threading.Tasks", 
                    "global::TheDialgaTeam.Mediator.Abstractions");
                
                #region Mediator
                
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
                            if (symbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Abstractions.IBaseRequest")))
                            {
                                var requestType = symbol.AllInterfaces.SingleOrDefault(typeSymbol => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).StartsWith("global::TheDialgaTeam.Mediator.Abstractions.IRequest"));

                                if (requestType is not null)
                                {
                                    if (requestType.IsGenericType)
                                    {
                                        constructorBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof(MediatorSender<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}, {string.Join(", ", requestType.TypeArguments.Select(typeSymbol => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))}>));");
                                    }
                                    else
                                    {
                                        constructorBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof(MediatorSender<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>));");
                                    }
                                }
                            }
                            
                            if (symbol.AllInterfaces.Any(namedTypeSymbol => namedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::TheDialgaTeam.Mediator.Abstractions.INotification")))
                            {
                                constructorBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof(MediatorPublisher<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>));");
                            }
                        }
                    });
                    
                    classBuilder.WriteLine("public Task SendAsync(IRequest request, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorSender>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.SendAsync(request, cancellationToken);");
                    });
                    
                    classBuilder.WriteLine("public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorSender<TResponse>>(_senders.GetOrAdd(request.GetType(), static (type, args) => Unsafe.As<IBaseMediatorSender>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.SendAsync(request, cancellationToken);");
                    });
                    
                    classBuilder.WriteLine("public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var handler = Unsafe.As<IMediatorPublisher>(_publishers.GetOrAdd(notification.GetType(), static (type, args) => Unsafe.As<IBaseMediatorPublisher>(args._serviceProvider.GetRequiredService(args._mediatorTypes[type])), (_serviceProvider, _mediatorTypes)));");
                        methodBuilder.WriteLine("return handler.PublishAsync(notification, cancellationToken);");
                    });
                });

                #endregion

                #region MediatorSender
                
                builder.WriteLine("file sealed class MediatorSender<TRequest> : IMediatorSender where TRequest : IRequest").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IRequestHandler<TRequest> _handler;");
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPipeline<TRequest>> _pipelines;");
                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest> handler) : this(handler, Array.Empty<IRequestPipeline<TRequest>>())").WriteBlock();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest> handler, IEnumerable<IRequestPipeline<TRequest>> pipelines)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handler = handler;");
                        constructorBuilder.WriteLine("_pipelines = pipelines;");
                    });
                    
                    classBuilder.WriteLine("public Task SendAsync(IRequest request, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return _pipelines.Reverse().Aggregate(() => _handler.HandleAsync((TRequest) request, cancellationToken), (next, pipeline) => () => pipeline.HandleAsync((TRequest) request, next, cancellationToken))();");
                    });
                });
                
                builder.WriteLine("file sealed class MediatorSender<TRequest, TResponse> : IMediatorSender<TResponse> where TRequest : IRequest<TResponse>").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IRequestHandler<TRequest, TResponse> _handler;");
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPipeline<TRequest, TResponse>> _pipelines;");
                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest, TResponse> handler) : this(handler, Array.Empty<IRequestPipeline<TRequest, TResponse>>())").WriteBlock();
                    
                    classBuilder.WriteLine("public MediatorSender(IRequestHandler<TRequest, TResponse> handler, IEnumerable<IRequestPipeline<TRequest, TResponse>> pipelines)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handler = handler;");
                        constructorBuilder.WriteLine("_pipelines = pipelines;");
                    });
                    
                    classBuilder.WriteLine("public Task<TResponse> SendAsync(IRequest<TResponse> request, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return _pipelines.Reverse().Aggregate(() => _handler.HandleAsync((TRequest) request, cancellationToken), (next, pipeline) => () => pipeline.HandleAsync((TRequest) request, next, cancellationToken))();");
                    });
                });

                #endregion

                #region MediatorPublisher
                
                builder.WriteLine("file sealed class MediatorPublisher<TNotification> : IMediatorPublisher where TNotification : INotification").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<INotificationHandler<TNotification>> _handlers;");
                    classBuilder.WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorPublisher() : this(Array.Empty<INotificationHandler<TNotification>>())").WriteBlock().WriteEmptyLine();
                    
                    classBuilder.WriteLine("public MediatorPublisher(IEnumerable<INotificationHandler<TNotification>> handlers)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_handlers = handlers;");
                    });
                    
                    classBuilder.WriteLine("public Task PublishAsync(INotification notification, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("return Task.WhenAll(_handlers.Select(handler => handler.HandleAsync((TNotification) notification, cancellationToken)));");
                    });
                });

                #endregion
            });

            sourceBuilder.WriteNamespace("TheDialgaTeam.Mediator.Implementations.Pipelines").WriteBlock(builder =>
            {
                builder.WriteUsingNamespace(
                    "global::System.Threading", 
                    "global::System.Threading.Tasks", 
                    "global::TheDialgaTeam.Mediator.Abstractions");
                
                #region PreProcessorRequestPipeline
                
                builder.WriteLine("file sealed class PreProcessorRequestPipeline<TRequest> : IRequestPipeline<TRequest> where TRequest : IRequest").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPreProcessor<TRequest>> _processors;");
                    classBuilder.WriteEmptyLine();

                    classBuilder.WriteLine("public PreProcessorRequestPipeline()").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = Array.Empty<IRequestPreProcessor<TRequest>>();");
                    });
                    
                    classBuilder.WriteLine("public PreProcessorRequestPipeline(IEnumerable<IRequestPreProcessor<TRequest>> processors)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = processors;");
                    });
                    
                    classBuilder.WriteLine("public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("foreach (var processor in _processors)").WriteBlock(foreachBuilder =>
                        {
                            foreachBuilder.WriteLine("await processor.Process(request, cancellationToken).ConfigureAwait(false);");
                        });

                        methodBuilder.WriteLine("await next().ConfigureAwait(false);");
                    });
                });
                
                builder.WriteLine("file sealed class PreProcessorRequestPipeline<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> where TRequest : IRequest<TResponse>").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPreProcessor<TRequest>> _processors;");
                    classBuilder.WriteEmptyLine();

                    classBuilder.WriteLine("public PreProcessorRequestPipeline()").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = Array.Empty<IRequestPreProcessor<TRequest>>();");
                    });
                    
                    classBuilder.WriteLine("public PreProcessorRequestPipeline(IEnumerable<IRequestPreProcessor<TRequest>> processors)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = processors;");
                    });
                    
                    classBuilder.WriteLine("public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("foreach (var processor in _processors)").WriteBlock(foreachBuilder =>
                        {
                            foreachBuilder.WriteLine("await processor.Process(request, cancellationToken).ConfigureAwait(false);");
                        });

                        methodBuilder.WriteLine("return await next().ConfigureAwait(false);");
                    });
                });

                #endregion

                #region PostProcessorRequestPipeline
                
                builder.WriteLine("file sealed class PostProcessorRequestPipeline<TRequest> : IRequestPipeline<TRequest> where TRequest : IRequest").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPostProcessor<TRequest>> _processors;");
                    classBuilder.WriteEmptyLine();

                    classBuilder.WriteLine("public PostProcessorRequestPipeline()").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = Array.Empty<IRequestPostProcessor<TRequest>>();");
                    });
                    
                    classBuilder.WriteLine("public PostProcessorRequestPipeline(IEnumerable<IRequestPostProcessor<TRequest>> processors)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = processors;");
                    });
                    
                    classBuilder.WriteLine("public async Task HandleAsync(TRequest request, Func<Task> next, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("await next().ConfigureAwait(false);");
                        
                        methodBuilder.WriteLine("foreach (var processor in _processors)").WriteBlock(foreachBuilder =>
                        {
                            foreachBuilder.WriteLine("await processor.Process(request, cancellationToken).ConfigureAwait(false);");
                        });
                    });
                });
                
                builder.WriteLine("file sealed class PostProcessorRequestPipeline<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> where TRequest : IRequest<TResponse>").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteLine("private readonly IEnumerable<IRequestPostProcessor<TRequest, TResponse>> _processors;");
                    classBuilder.WriteEmptyLine();

                    classBuilder.WriteLine("public PostProcessorRequestPipeline()").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = Array.Empty<IRequestPostProcessor<TRequest, TResponse>>();");
                    });
                    
                    classBuilder.WriteLine("public PostProcessorRequestPipeline(IEnumerable<IRequestPostProcessor<TRequest, TResponse>> processors)").WriteBlock(constructorBuilder =>
                    {
                        constructorBuilder.WriteLine("_processors = processors;");
                    });
                    
                    classBuilder.WriteLine("public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)").WriteBlock(methodBuilder =>
                    {
                        methodBuilder.WriteLine("var response = await next().ConfigureAwait(false);");
                        
                        methodBuilder.WriteLine("foreach (var processor in _processors)").WriteBlock(foreachBuilder =>
                        {
                            foreachBuilder.WriteLine("await processor.Process(request, response, cancellationToken).ConfigureAwait(false);");
                        });

                        methodBuilder.WriteLine("return response;");
                    });
                });

                #endregion
            });

            productionContext.AddSource("Mediator.cs", sourceBuilder.ToString());
        });
    }
}