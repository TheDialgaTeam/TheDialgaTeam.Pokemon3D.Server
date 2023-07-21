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

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheDialgaTeam.Pokemon3D.Server.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    private const string AddMediatorHandlersAttribute = "TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Attributes.AddMediatorHandlersAttribute";
    private const string NotificationFullyQualifiedName = "global::TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Interfaces.INotification";
    private const string MediatorPublisherFullyQualifiedName = "global::TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations.MediatorPublisher";
    
    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var getMediatorInterfaces = context.CompilationProvider.Select((compilation, token) => compilation.SyntaxTrees
            .SelectMany(tree => tree.GetRoot(token).DescendantNodes())
            .Where(node => node is ClassDeclarationSyntax or RecordDeclarationSyntax)
            .Select(node => compilation.GetSemanticModel(node.SyntaxTree).GetDeclaredSymbol(node, token))
            .Where(symbol => (symbol as ITypeSymbol)?.AllInterfaces.Any(typeSymbol => typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals(NotificationFullyQualifiedName)) ?? false)
            .ToImmutableArray()
        );
        
        var getAllMediatorHandlers = context.SyntaxProvider.ForAttributeWithMetadataName(AddMediatorHandlersAttribute,
            (node, _) => node is MemberDeclarationSyntax,
            (syntaxContext, token) => syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.TargetNode, token));

        context.RegisterSourceOutput(getMediatorInterfaces, (productionContext, symbols) =>
        {
            var sourceBuilder = new SourceBuilder();
            sourceBuilder.WriteNamespace("TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Implementations").WriteBlock(builder =>
            {
                builder.WriteGeneratedCodeAttribute();
                builder.WriteLine("partial class Mediator").WriteBlock(classBuilder =>
                {
                    classBuilder.WriteGeneratedCodeAttribute();
                    classBuilder.WriteLine("private partial void Initialize()").WriteBlock(methodBuilder =>
                    {
                        foreach (var symbol in symbols)
                        {
                            if (symbol == null) continue;
                            methodBuilder.WriteLine($"_mediatorTypes.Add(typeof({symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), typeof({MediatorPublisherFullyQualifiedName}<{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>));");
                        }
                    });
                });
            });
            
            productionContext.AddSource("Mediator.g.cs", sourceBuilder.ToString());
        });
        
        context.RegisterSourceOutput(getAllMediatorHandlers, (productionContext, symbol) =>
        {
            if (symbol is not IMethodSymbol methodSymbol) return;
            if (!methodSymbol.IsPartialDefinition) return;
            if (methodSymbol.Parameters.Length != 1) return;
            if (!methodSymbol.Parameters[0].Type.ToDisplayString().Equals("Microsoft.Extensions.DependencyInjection.IServiceCollection")) return;
            
            var accessibility = methodSymbol.DeclaredAccessibility switch
            {
                Accessibility.Public => "public ",
                Accessibility.ProtectedAndInternal => "protected internal ",
                Accessibility.Protected => "protected ",
                Accessibility.Internal => "internal ",
                Accessibility.Private => "private ",
                var _ => ""
            };

            var queryHandlerTypes = methodSymbol.GetAttributes()
                .Where(attributeData => attributeData.AttributeClass?.ToDisplayString().Equals(AddMediatorHandlersAttribute) ?? false)
                .Select(attributeData => attributeData.ConstructorArguments.Select(argument => argument.Value).ToArray())
                .Select(args => (args[0] as ITypeSymbol, args[1]))
                .SelectMany(tuple => tuple.Item1?.AllInterfaces
                    .Where(typeParameterSymbol => typeParameterSymbol.IsGenericType && typeParameterSymbol.Name is "IRequestHandler" or "INotificationHandler")
                    .Select(typeParameterSymbol =>
                    {
                        return typeParameterSymbol.Name switch
                        {
                            "IRequestHandler" => $"{methodSymbol.Parameters[0].Name}.TryAdd(ServiceDescriptor.Describe(typeof({typeParameterSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), static provider => provider.GetRequiredService<{tuple.Item1.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(), {tuple.Item2}));",
                            "INotificationHandler" => $"{methodSymbol.Parameters[0].Name}.Add(ServiceDescriptor.Describe(typeof({typeParameterSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}), static provider => provider.GetRequiredService<{tuple.Item1.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}>(), {tuple.Item2}));",
                            var _ => string.Empty
                        };
                    })
                )
                .ToImmutableArray();

            var sourceBuilder = new SourceBuilder();
            sourceBuilder.WriteUsingNamespace("global::Microsoft.Extensions.DependencyInjection", "global::Microsoft.Extensions.DependencyInjection.Extensions");
            sourceBuilder.WriteNamespace(methodSymbol.ContainingNamespace.ToDisplayString());
            sourceBuilder.WriteOpenBlock();

            var innerTypeCount = 0;
            
            IEnumerable<INamedTypeSymbol> GetContainingTypes(ISymbol? s)
            {
                if (s != null)
                {
                    yield return s.ContainingType;
                }
            }
            
            foreach (var namedTypeSymbol in GetContainingTypes(methodSymbol).Reverse())
            {
                sourceBuilder.WriteGeneratedCodeAttribute();
                sourceBuilder.WriteLine($"{(namedTypeSymbol.IsStatic ? "static " : "")}partial class {namedTypeSymbol.Name}");
                sourceBuilder.WriteOpenBlock();
                innerTypeCount++;
            }

            sourceBuilder.WriteGeneratedCodeAttribute();
            sourceBuilder.WriteLine($"{accessibility}{(methodSymbol.IsStatic ? "static " : "")}partial {methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {methodSymbol.Name}({methodSymbol.Parameters[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.AddParameterOptions(~SymbolDisplayParameterOptions.None))})");
            sourceBuilder.WriteBlock(builder =>
            {
                foreach (var queryHandlerType in queryHandlerTypes)
                {
                    builder.WriteLine(queryHandlerType);
                }
            });

            for (var i = 0; i < innerTypeCount; i++)
            {
                sourceBuilder.WriteCloseBlock();
            }
            
            sourceBuilder.WriteCloseBlock();
            
            productionContext.AddSource($"{methodSymbol.Name}.{methodSymbol.GetHashCode().ToString()}.g.cs", sourceBuilder.ToString());
        });
    }
}