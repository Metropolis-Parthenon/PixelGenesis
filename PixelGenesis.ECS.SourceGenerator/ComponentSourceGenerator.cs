using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PixelGenesis.ECS.SourceGenerator;

[Generator]
public class ComponentSourceGenerator : IIncrementalGenerator
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

        generatedSource.AppendLine("using PixelGenesis.ECS.Components;");
        
        generatedSource.AppendLine();

        foreach (var @class in distinctClasses)
        {
            if (@class is null)
            {
                continue;
            }
           
            var semanticModel = compilation.GetSemanticModel(@class.SyntaxTree);

            var namespaceDeclaration = GetNamespace(@class);
            generatedSource.AppendLine($"namespace {namespaceDeclaration}");
            generatedSource.AppendLine("{");

            var publicFields = @class.Members.OfType<FieldDeclarationSyntax>().Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)));

            generatedSource.AppendLine($"   public sealed partial class {@class.Identifier.Text} : Component");
            generatedSource.AppendLine("    {");

            generatedSource.AppendLine("        public override void CopyToAnother(Component component)");
            generatedSource.AppendLine("        {");
            generatedSource.AppendLine($"            var other = ({@class.Identifier.Text})component;");
            foreach (var field in publicFields)
            {
                var fieldName = field.Declaration.Variables.First().Identifier.Text;
            generatedSource.AppendLine($"            other.{fieldName} = {fieldName};");
            }
            generatedSource.AppendLine("        }");


            generatedSource.AppendLine("        public override IEnumerable<KeyValuePair<string, object>> GetSerializableValues()");
            generatedSource.AppendLine("        {");

            if(!publicFields.Any())
            {
               generatedSource.AppendLine("            yield break;");
            }
            else 
            {            
            foreach(var field in publicFields)
            {
                var fieldName = field.Declaration.Variables.First().Identifier.Text;
            generatedSource.AppendLine($"           yield return new KeyValuePair<string, object>(nameof({fieldName}), {fieldName});");
            
            }
            }
            generatedSource.AppendLine("        }");

            generatedSource.AppendLine("        public override void SetSerializableValues(IEnumerable<KeyValuePair<string, object>> values)");
            generatedSource.AppendLine("        {");
            generatedSource.AppendLine("            foreach(var kvp in values)");
            generatedSource.AppendLine("            {");
            generatedSource.AppendLine("                switch(kvp.Key)");
            generatedSource.AppendLine("                {");
            foreach (var field in publicFields)
            {
                var fieldName = field.Declaration.Variables.First().Identifier.Text;
            generatedSource.AppendLine($"                     case nameof({fieldName}):");
            generatedSource.AppendLine($"                        {fieldName} = ({GetTypeFullName(semanticModel, field.Declaration.Type)})kvp.Value;");
            generatedSource.AppendLine("                      break;");
            }
            generatedSource.AppendLine("                }");
            generatedSource.AppendLine("            }");
            generatedSource.AppendLine("        }");


            generatedSource.AppendLine("        public override System.Type GetPropType(string key)");
            generatedSource.AppendLine("        {");            
            generatedSource.AppendLine("            switch(key)");
            generatedSource.AppendLine("            {");
            foreach (var field in publicFields)
            {
                var fieldName = field.Declaration.Variables.First().Identifier.Text;
            generatedSource.AppendLine($"               case nameof({fieldName}):");
            generatedSource.AppendLine($"                   return typeof({GetTypeFullName(semanticModel, field.Declaration.Type)});");            
            }
            generatedSource.AppendLine("                default: throw new System.InvalidOperationException();");
            generatedSource.AppendLine("            }");            
            generatedSource.AppendLine("        }");

            generatedSource.AppendLine("       public override int GetOwnerIndex()");
            generatedSource.AppendLine("       {");
            generatedSource.AppendLine($"           return Entity.Index;");
            generatedSource.AppendLine("       }");

            generatedSource.AppendLine("    }");

            StringBuilder constructorParameters = new StringBuilder();
            if (@class.ParameterList is not null)
            {                
                bool isFirst = true;
                foreach (var item in @class.ParameterList.Parameters)
                {
                    if (item.Type is null)
                    {
                        continue;
                    }

                    if (!isFirst)
                    {
                        constructorParameters.Append(", ");
                    }
                    isFirst = false;
                    constructorParameters.Append($"resolver.Resolve<{GetTypeFullName(semanticModel, item.Type)}>()");
                }
            }

            generatedSource.AppendLine($"    public class {@class.Identifier.Text}Factory : IComponentFactory");
            generatedSource.AppendLine("    {");
            generatedSource.AppendLine("        public Component CreateComponent(ComponentDependencyResolver resolver)");
            generatedSource.AppendLine("        {");
            generatedSource.AppendLine($"            return new {@class.Identifier.Text}({constructorParameters});");
            generatedSource.AppendLine("        }");
            generatedSource.AppendLine("    }");



            generatedSource.AppendLine("}");
        }



        context.AddSource("Components.g.cs", SourceText.From(generatedSource.ToString(), Encoding.UTF8));
    }

    static string GetTypeFullName(SemanticModel semanticModel, TypeSyntax typeSyntax)
    {        
        var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol;
                
        if(typeSymbol is null)
        {            
            return "";
        }

        if(typeSymbol.Kind is SymbolKind.ArrayType && typeSymbol is IArrayTypeSymbol arr)
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

        if(classDeclaration.BaseList.Types.Any(t => t.Type is IdentifierNameSyntax identifierNameSyntax && identifierNameSyntax.Identifier.Text == "Component"))
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


//public class Transform3DComponentFactory : IComponentFactory
//{
//    public Component CreateComponent(ComponentDependencyResolver resolver)
//    {
//        return new Transform3DComponent(resolver.Resolve<Transform3DComponent>());
//    }
//}