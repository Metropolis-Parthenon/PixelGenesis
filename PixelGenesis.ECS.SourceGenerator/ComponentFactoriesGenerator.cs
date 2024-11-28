using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        generatedSource.AppendLine();

        generatedSource.AppendLine("namespace PixelGenesis._3D.Common;");

        generatedSource.AppendLine("public static class ComponentInitializer");
        generatedSource.AppendLine("{");
        generatedSource.AppendLine("    public static void Initialize()");
        generatedSource.AppendLine("    {");
            
        foreach(var @class in distinctClasses)
        {
            if(@class is null)
            {
                continue;
            }

            var classFullName = $"{GetNamespace(@class)}.{@class.Identifier.Text}";


            StringBuilder constructorParameters = new StringBuilder();
            if(@class.ParameterList is not null)
            {
                var semanticModel = compilation.GetSemanticModel(@class.SyntaxTree);
                bool isFirst = true;
                foreach (var item in @class.ParameterList.Parameters)
                {
                    if(item.Type is null)
                    {
                        continue;
                    }

                    if (!isFirst)
                    {
                        constructorParameters.Append(", ");
                    }
                    isFirst = false;
                    constructorParameters.Append($"entity.AddComponentIfNotExist<{GetTypeFullName(semanticModel, item.Type)}>()");
                }
            }

            generatedSource.AppendLine($"       ComponentFactory.AddComponentFactory(typeof({classFullName}), entity => new {classFullName}({constructorParameters}));");

        }

        generatedSource.AppendLine("    }");
        generatedSource.AppendLine("}");

        context.AddSource("ComponentInitializer.g.cs", SourceText.From(generatedSource.ToString(), Encoding.UTF8));
    }


    static string GetTypeFullName(SemanticModel semanticModel, TypeSyntax typeSyntax)
    {
        var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol;

        if (typeSymbol is null)
        {
            return "";
        }

        if (typeSymbol.Kind is SymbolKind.ArrayType && typeSymbol is IArrayTypeSymbol arr)
        {
            return $"{arr.ElementType.Name}[]";
        }

        return $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";
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


//public static class ComponentInitializer
//{
//    public static void Initialize()
//    {
//        ComponentFactory.AddComponentFactory(typeof(Transform3DComponent), entity => new Transform3DComponent());
//        ComponentFactory.AddComponentFactory(typeof(MeshRendererComponent), entity => new MeshRendererComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(CubeRendererComponent), entity => new CubeRendererComponent(entity.AddComponentIfNotExist<MeshRendererComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(SphereRendererComponent), entity => new SphereRendererComponent(entity.AddComponentIfNotExist<MeshRendererComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(PerspectiveCameraComponent), entity => new PerspectiveCameraComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(DirectionalLightComponent), entity => new DirectionalLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(PointLightComponent), entity => new PointLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//        ComponentFactory.AddComponentFactory(typeof(SpotLightComponent), entity => new SpotLightComponent(entity.AddComponentIfNotExist<Transform3DComponent>()));
//    }
//}
