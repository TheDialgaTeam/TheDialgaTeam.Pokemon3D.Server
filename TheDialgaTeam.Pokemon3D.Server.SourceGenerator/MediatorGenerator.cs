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

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TheDialgaTeam.Pokemon3D.Server.SourceGenerator;

[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    private const string QueryHandlerAttribute = "TheDialgaTeam.Pokemon3D.Server.Core.Mediator.Attributes.QueryHandlerAttribute";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var queryHandlerAttributeMethod = context.SyntaxProvider.ForAttributeWithMetadataName(QueryHandlerAttribute, 
            (node, _) => node is MemberDeclarationSyntax,
            (syntaxContext, token) => syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.TargetNode, token) as IMethodSymbol);

        context.RegisterSourceOutput(queryHandlerAttributeMethod, (productionContext, symbol) =>
        {
            if (symbol == null) return;
            if (!symbol.IsPartialDefinition) return;
            if (symbol.Parameters.IsEmpty || symbol.Parameters.Length > 1) return;
            if (!symbol.Parameters[0].Type.ToDisplayString().Equals("Microsoft.Extensions.DependencyInjection.IServiceCollection")) return;
            
            var accessibility = symbol.DeclaredAccessibility switch
            {
                Accessibility.Private => "private",
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                var _ => ""
            };
            
            var queryHandlerTypes = symbol.GetAttributes()
                .Where(data => data.AttributeClass?.ToDisplayString().Equals(QueryHandlerAttribute) ?? false)
                .SelectMany(data => data.ConstructorArguments.Select(constant => constant.Value))
                .OfType<ITypeSymbol>()
                .SelectMany(typeSymbol => typeSymbol.AllInterfaces
                    .Where(namedTypeSymbol => namedTypeSymbol.IsGenericType && namedTypeSymbol.Name == "IRequestHandler")
                    .Select(namedTypeSymbol => $"            {symbol.Parameters[0].Name}.TryAddSingleton<{namedTypeSymbol.ToDisplayString()}>(static provider => provider.GetRequiredService<{typeSymbol.ToDisplayString()}>());"));


            var sourceOutputBuilder = new StringBuilder();
            sourceOutputBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sourceOutputBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection.Extensions;");
            sourceOutputBuilder.AppendLine(string.Empty);
            sourceOutputBuilder.AppendLine($"namespace {symbol.ContainingNamespace.ToDisplayString()}");
            sourceOutputBuilder.AppendLine("{");
            sourceOutputBuilder.AppendLine("    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"TheDialgaTeam.Pokemon3D.Server.SourceGenerator\", \"1.0.0\")]");
            sourceOutputBuilder.AppendLine($"    {(symbol.ContainingType.IsStatic ? "static" : "")} partial class {symbol.ContainingType.Name}");
            sourceOutputBuilder.AppendLine("    {");
            sourceOutputBuilder.AppendLine("        [global::System.CodeDom.Compiler.GeneratedCode(\"TheDialgaTeam.Pokemon3D.Server.SourceGenerator\", \"1.0.0\")]");
            sourceOutputBuilder.AppendLine($"        {accessibility} {(symbol.IsStatic ? "static " : "")}partial {symbol.ReturnType.ToDisplayString()} {symbol.Name}({symbol.Parameters[0].ToDisplayString()})");
            sourceOutputBuilder.AppendLine("        {");

            foreach (var queryHandlerType in queryHandlerTypes)
            {
                sourceOutputBuilder.AppendLine(queryHandlerType);
            }
            
            sourceOutputBuilder.AppendLine("        }");
            sourceOutputBuilder.AppendLine("    }");
            sourceOutputBuilder.Append("}");
            
            productionContext.AddSource($"{symbol.Name}.{symbol.GetHashCode().ToString()}.g.cs", sourceOutputBuilder.ToString());
        });
    }
}