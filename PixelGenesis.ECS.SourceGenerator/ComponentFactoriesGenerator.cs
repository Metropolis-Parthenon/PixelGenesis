using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace PixelGenesis.ECS.SourceGenerator;

[Generator]
public class ComponentFactoriesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsComponentClass(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
            ).Where(static m => m is not null);

        var compilationAndClasses
             = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }


    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        var distinctClasses = classes.Distinct();

        var generatedSource = new StringBuilder();

        generatedSource.AppendLine("using PixelGenesis.ECS;");
        generatedSource.AppendLine("using PixelGenesis.ECS.Components;");        
        generatedSource.AppendLine("using Microsoft.Extensions.DependencyInjection;");

        generatedSource.AppendLine();

        var namespaces = distinctClasses.Select(x => GetNamespace(x));

        var commonPrefix = new string(
            namespaces.First().Substring(0, namespaces.Min(s => s.Length))
            .TakeWhile((c, i) => namespaces.All(s => s[i] == c)).ToArray());

        generatedSource.AppendLine($"namespace {commonPrefix};");

        generatedSource.AppendLine("public static class ServiceCollectionComponentFactoriesExtensions");
        generatedSource.AppendLine("{");
        generatedSource.AppendLine("    public static void AddComponentFactories(this IServiceCollection services)");
        generatedSource.AppendLine("    {");
            
        foreach(var @class in distinctClasses)
        {
            if(@class is null)
            {
                continue;
            }

            var classFullName = $"{GetNamespace(@class)}.{@class.Identifier.Text}";
            
        generatedSource.AppendLine($"           services.AddPixelGenesisComponentFactory<{classFullName}, {classFullName}Factory>();");

        }

        generatedSource.AppendLine("    }");
        generatedSource.AppendLine("}");

        context.AddSource("ComponentInitializer.g.cs", SourceText.From(generatedSource.ToString(), Encoding.UTF8));
    }

    static bool IsComponentClass(SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
        && classDeclarationSyntax?.BaseList?.Types.Count > 0;


    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = context.Node as ClassDeclarationSyntax;
        if (classDeclaration?.BaseList is null)
        {
            return null;
        }

        if (classDeclaration.BaseList.Types.Any(t => t.Type is IdentifierNameSyntax identifierNameSyntax && identifierNameSyntax.Identifier.Text == "Component"))
        {
            return classDeclaration;
        }

        return null;
    }


    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

}


//public static class ServiceCollectionExtensions
//{
//    public static void AddComponentFactories(this IServiceCollection services)
//    {
//        services.AddPixelGenesisComponentFactory<Transform3DComponent, Transform3DComponentFactory>();
//    }
//}