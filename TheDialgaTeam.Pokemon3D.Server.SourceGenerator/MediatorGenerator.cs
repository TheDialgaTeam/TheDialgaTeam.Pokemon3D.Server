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
    private const string CommandHandlerAttribute = "TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Attributes.CommandHandlerAttribute";
    private const string QueryHandlerAttribute = "TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Attributes.QueryHandlerAttribute";
    
    [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality")]
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var getCommandHandlerMethods = context.SyntaxProvider.ForAttributeWithMetadataName(CommandHandlerAttribute, 
            (node, _) => node is MemberDeclarationSyntax,
            (syntaxContext, token) => syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.TargetNode, token));

        var getQueryHandlerMethods = context.SyntaxProvider.ForAttributeWithMetadataName(QueryHandlerAttribute, 
            (node, _) => node is MemberDeclarationSyntax,
            (syntaxContext, token) => syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.TargetNode, token));
        
        context.RegisterSourceOutput(getCommandHandlerMethods, (productionContext, symbol) =>
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
            
            var commandHandlerTypes = methodSymbol.GetAttributes()
                .Where(attributeData => attributeData.AttributeClass?.ToDisplayString().Equals(CommandHandlerAttribute) ?? false)
                .SelectMany(attributeData => attributeData.ConstructorArguments.Select(argument => argument.Value))
                .OfType<ITypeSymbol>()
                .SelectMany(typeSymbol => typeSymbol.AllInterfaces
                    .Where(namedTypeSymbol => namedTypeSymbol.IsGenericType && namedTypeSymbol is { Name: "IRequestHandler", TypeParameters.Length: 1 })
                    .Select(namedTypeSymbol => $"{methodSymbol.Parameters[0].Name}.TryAddSingleton<{namedTypeSymbol.ToDisplayString()}>(static provider => provider.GetRequiredService<{typeSymbol.ToDisplayString()}>());"))
                .ToImmutableArray();

            var sourceBuilder = new SourceBuilder();
            sourceBuilder.WriteLine("using Microsoft.Extensions.DependencyInjection;");
            sourceBuilder.WriteLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
            sourceBuilder.WriteEmptyLine();
            sourceBuilder.WriteLine($"namespace {methodSymbol.ContainingNamespace.ToDisplayString()}");
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
                sourceBuilder.WriteLine("[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"TheDialgaTeam.Pokemon3D.Server.SourceGenerator\", \"1.0.0\")]");
                sourceBuilder.WriteLine($"{(namedTypeSymbol.IsStatic ? "static" : "")} partial class {namedTypeSymbol.Name}");
                sourceBuilder.WriteOpenBlock();
                innerTypeCount++;
            }

            sourceBuilder.WriteLine("[global::System.CodeDom.Compiler.GeneratedCode(\"TheDialgaTeam.Pokemon3D.Server.SourceGenerator\", \"1.0.0\")]");
            sourceBuilder.WriteLine($"{accessibility}{(methodSymbol.IsStatic ? "static " : "")}partial {methodSymbol.ReturnType.ToDisplayString()} {methodSymbol.Name}({methodSymbol.Parameters[0].ToDisplayString()})");
            sourceBuilder.WriteOpenBlock();

            foreach (var queryHandlerType in commandHandlerTypes)
            {
                sourceBuilder.WriteLine(queryHandlerType);
            }
            
            sourceBuilder.WriteCloseBlock();
            
            for (var i = 0; i < innerTypeCount; i++)
            {
                sourceBuilder.WriteCloseBlock();
            }
            
            sourceBuilder.WriteCloseBlock();
            
            productionContext.AddSource($"{methodSymbol.Name}.{methodSymbol.GetHashCode().ToString()}.g.cs", sourceBuilder.ToString());
        });
        
        context.RegisterSourceOutput(getQueryHandlerMethods, (productionContext, symbol) =>
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
                .Where(attributeData => attributeData.AttributeClass?.ToDisplayString().Equals(QueryHandlerAttribute) ?? false)
                .SelectMany(attributeData => attributeData.ConstructorArguments.Select(argument => argument.Value))
                .OfType<ITypeSymbol>()
                .SelectMany(typeSymbol => typeSymbol.AllInterfaces
                    .Where(namedTypeSymbol => namedTypeSymbol.IsGenericType && namedTypeSymbol is { Name: "IRequestHandler", TypeParameters.Length: 2 })
                    .Select(namedTypeSymbol => $"{methodSymbol.Parameters[0].Name}.TryAddSingleton<{namedTypeSymbol.ToDisplayString()}>(static provider => provider.GetRequiredService<{typeSymbol.ToDisplayString()}>());"))
                .ToImmutableArray();

            var sourceBuilder = new SourceBuilder();
            sourceBuilder.WriteLine("using Microsoft.Extensions.DependencyInjection;");
            sourceBuilder.WriteLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
            sourceBuilder.WriteEmptyLine();
            sourceBuilder.WriteLine($"namespace {methodSymbol.ContainingNamespace.ToDisplayString()}");
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
                sourceBuilder.WriteLine($"{(namedTypeSymbol.IsStatic ? "static" : "")} partial class {namedTypeSymbol.Name}");
                sourceBuilder.WriteOpenBlock();
                innerTypeCount++;
            }

            sourceBuilder.WriteGeneratedCodeAttribute();
            sourceBuilder.WriteLine($"{accessibility}{(methodSymbol.IsStatic ? "static " : "")}partial {methodSymbol.ReturnType.ToDisplayString()} {methodSymbol.Name}({methodSymbol.Parameters[0].ToDisplayString()})");
            sourceBuilder.WriteOpenBlock();

            foreach (var queryHandlerType in queryHandlerTypes)
            {
                sourceBuilder.WriteLine(queryHandlerType);
            }
            
            sourceBuilder.WriteCloseBlock();
            
            for (var i = 0; i < innerTypeCount; i++)
            {
                sourceBuilder.WriteCloseBlock();
            }
            
            sourceBuilder.WriteCloseBlock();
            
            productionContext.AddSource($"{methodSymbol.Name}.{methodSymbol.GetHashCode().ToString()}.g.cs", sourceBuilder.ToString());
        });
    }
}