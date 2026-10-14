namespace Muify.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MooVC;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;
    using BinderStructExtensions = Muify.Syntax.CSharp.StructExtensions;
    using TimeSpan = System.TimeSpan;

    internal sealed class GenerateBinderWhenFeatureReferenceVisited
        : IModelVisitor<Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco, File>
    {
        public static bool CanGenerateBinder(Poco poco)
        {
            return poco.IsAccessible
                && poco.IsPartial
                && !poco.HasBinder
                && !poco.HasDomainBinder
                && !poco.Qualification.IsUnnamed
                && !poco.Characteristics.IsUndefined
                && !poco.IsUndefined;
        }

        public static IEnumerable<File> Generate(Poco poco)
        {
            if (!CanGenerateBinder(poco))
            {
                yield break;
            }

            Snippet bindings = ApplyBindings(poco);
            Type declaration = CreateBinder(poco, bindings);

            foreach (Poco containingType in poco.ContainingTypes)
            {
                declaration = WrapInContainingType(declaration, containingType);
            }

            string content = Define(declaration)
                .From(poco.Qualification.Qualifier)
                .ToSnippet(Configuration.Options);

            content = NormalizeRecordStructDeclarations(content);
            content = ApplyBinderInterface(content, poco);

            yield return new File(content, $"{poco.Qualification.Qualifier}.{poco.Qualification.Moniker}.Binder");
        }

        public IEnumerable<File> Observe(Model.Graph.Areas.Area.Units.Unit.Features.Feature.Metadata.References.Poco poco)
        {
            return Generate(poco._Value);
        }

        private static string ApplyBinderInterface(string content, Poco poco)
        {
            if (!poco.Characteristics.IsStruct)
            {
                return content;
            }

            // MooVC's struct renderer omits interfaces, so preserve its output and add the binder contract.
            TypeDeclarationSyntax declaration = CSharpSyntaxTree.ParseText(content)
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Single(type => GetMoniker(type) == poco.Qualification.Moniker.ToString());

            int lineStart = content.LastIndexOf('\n', declaration.SpanStart) + 1;
            string indentation = content.Substring(lineStart, declaration.SpanStart - lineStart);
            string lineBreak = declaration.Identifier.TrailingTrivia
                .First(trivia => trivia.RawKind == (int)SyntaxKind.EndOfLineTrivia)
                .ToString();
            Symbol binder = (Name: "IBinder", Qualifier: "Mu.Serialization");

            return content.Insert(declaration.Identifier.Span.End, $"{lineBreak}{indentation}    : {Configuration.Render(binder)}");
        }

        private static Snippet ApplyBindings(Poco poco)
        {
            var bindings = new List<string>
            {
                $"var meta = model.Add(typeof({poco.Qualification.Moniker}), false);",
                string.Empty,
                "meta.UseConstructor = false;",
                string.Empty,
            };

            int index = 4;

            foreach (Attribute property in poco.Attributes)
            {
                bindings.Add($"meta.Add({index++}, \"{property.Name}\");");
            }

            bindings.Add(string.Empty);
            bindings.Add("return model;");

            return bindings.ToSnippet(Configuration.Options);
        }

        private static Type CreateBinder(Poco poco, Snippet bindings)
        {
            Name name = GetName(poco);

            if (poco.Characteristics.IsStruct)
            {
                return Struct.Undefined.WithBinder(bindings, poco.Characteristics, name);
            }

            if (poco.Characteristics.IsRecord)
            {
                return Record.Undefined.WithBinder(bindings, name);
            }

            return Class.Undefined.WithBinder(bindings, name);
        }

        private static Definition Define(Type declaration)
        {
            if (declaration is Record record)
            {
                return Builder.New<Definition>().For<Record>(_ => record);
            }

            if (declaration is Struct @struct)
            {
                return Builder.New<Definition>().For<Struct>(_ => @struct);
            }

            return Builder.New<Definition>().For<Class>(_ => (Class)declaration);
        }

        private static string GetMoniker(TypeDeclarationSyntax declaration)
        {
            IEnumerable<string> names = declaration.Ancestors()
                .OfType<TypeDeclarationSyntax>()
                .Reverse()
                .Select(type => type.Identifier.ValueText)
                .Append(declaration.Identifier.ValueText);

            return string.Join(".", names);
        }

        private static Name GetName(Poco poco)
        {
            string moniker = poco.Qualification.Moniker.ToString();

            return moniker.Substring(moniker.LastIndexOf('.') + 1);
        }

        private static string NormalizeRecordStructDeclarations(string content)
        {
            // MooVC renders mutable record structs with the record keyword before the partial modifier.
            return Regex.Replace(
                content,
                @"^(?<Indentation>[ \t]*)record[ \t]+partial[ \t]+struct(?=[ \t])",
                "${Indentation}partial record struct",
                RegexOptions.Multiline,
                TimeSpan.FromSeconds(1));
        }

        private static Type WrapInContainingType(Type declaration, Poco containingType)
        {
            Name name = GetName(containingType);

            if (containingType.Characteristics.IsStruct)
            {
                return Struct.Undefined
                    .Containing(declaration)
                    .Named(name)
                    .WithBehavior(BinderStructExtensions.GetBehavior(containingType.Characteristics))
                    .WithScope(Scopes.Unspecified);
            }

            if (containingType.Characteristics.IsRecord)
            {
                return Record.Undefined
                    .Containing(declaration)
                    .Named(name)
                    .WithExtensibility(Modifiers.Implicit)
                    .WithScope(Scopes.Unspecified);
            }

            return Class.Undefined
                .Containing(declaration)
                .Named(name)
                .WithExtensibility(Modifiers.Implicit)
                .WithScope(Scopes.Unspecified);
        }
    }
}