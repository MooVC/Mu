namespace Muify
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using MooVC.Syntax;
    using Mu.Modelling;

    [Generator(LanguageNames.CSharp)]
    public sealed partial class ModelGenerator
        : IIncrementalGenerator
    {
        private static readonly IServiceProvider _provider = new ServiceProvider();

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValueProvider<Model> models = context.CompilationProvider.Select(GetModel);

            context.RegisterSourceOutput(models, Generate);
        }

        private static void Generate(SourceProductionContext context, Model model)
        {
            var navigator = new ModelNavigator(_provider);
            IEnumerable<File> results = navigator.Navigate<File>(model);

            foreach (File result in results)
            {
                var source = SourceText.From(result.Content, Encoding.UTF8);

                context.AddSource(result.Hint, source);
            }
        }

        private static Model GetModel(Compilation compilation, CancellationToken cancellationToken)
        {
            string assemblyName = compilation.AssemblyName ?? string.Empty;
            string[] segments = assemblyName.Split('.');

            if (segments.Length < 4)
            {
                return Model.Undefined;
            }

            Model model = Model.Undefined
                .For(segments[0])
                .Named(segments[1]);

            if (segments.Length == 5)
            {
                return GetFeatureModel(compilation, cancellationToken, segments, model);
            }

            if (segments.Length == 4)
            {
                return GetDomainModel(compilation, cancellationToken, segments, model);
            }

            return model;
        }

        private static Model GetFeatureModel(Compilation compilation, CancellationToken cancellationToken, IReadOnlyList<string> segments, Model model)
        {
            INamedTypeSymbol request = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{segments[4]}");

            if (request is null)
            {
                return model;
            }

            var feature = Feature.Undefined
                .Named(segments[4]);

            foreach (INamedTypeSymbol member in request.GetTypeMembers().Where(type => type.IsRecord))
            {
                feature = feature.Returning(result => result
                    .Named(member.Name)
                    .OfType((Name: member.Name, Qualifier: member.ContainingNamespace.ToDisplayString())));
            }

            var unit = Unit.Undefined
                .Named(segments[3])
                .Featuring(feature);

            var area = Area.Undefined
                .Named(segments[2])
                .ResponsibleFor(unit);

            cancellationToken.ThrowIfCancellationRequested();

            return model.Defines(area);
        }

        private static Model GetDomainModel(Compilation compilation, CancellationToken cancellationToken, IReadOnlyList<string> segments, Model model)
        {
            INamedTypeSymbol aggregate = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{segments[3]}");

            if (aggregate is null || !aggregate.IsRecord)
            {
                return model;
            }

            var components = new List<Component>();

            foreach (ITypeSymbol type in GetReferencedTypes(aggregate))
            {
                cancellationToken.ThrowIfCancellationRequested();

                INamedTypeSymbol namedType = type as INamedTypeSymbol;

                if (namedType is null || namedType.SpecialType != SpecialType.None)
                {
                    continue;
                }

                if (namedType.IsRecord)
                {
                    components.Add(Component.Undefined.Named(namedType.Name));
                    continue;
                }

                if (namedType.TypeKind != TypeKind.Class)
                {
                    continue;
                }

                Mu.Modelling.Attribute identifier = GetIdentity(namedType);
                Component component = Component.Undefined.Named(namedType.Name);

                if (!identifier.IsUndefined)
                {
                    component = component.IdentifiedBy(attribute => attribute
                        .Named(identifier.Name)
                        .OfType(identifier.Type));
                }

                components.Add(component);
            }

            var unit = Unit.Undefined
                .Named(segments[3]);

            var area = Area.Undefined
                .Named(segments[2])
                .ResponsibleFor(unit);

            foreach (Component component in components)
            {
                area = area.Owns(_ => component);
            }

            return model.Defines(area);
        }

        private static Mu.Modelling.Attribute GetIdentity(INamedTypeSymbol symbol)
        {
            IPropertySymbol identity = symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(property => property.GetAttributes().Any(attribute => IsIdentityAttribute(attribute.AttributeClass)));

            if (identity is null)
            {
                return Mu.Modelling.Attribute.Undefined;
            }

            return Mu.Modelling.Attribute.Undefined
                .Named(identity.Name)
                .OfType((Name: identity.Type.Name, Qualifier: identity.Type.ContainingNamespace.ToDisplayString()));
        }

        private static IEnumerable<ITypeSymbol> GetReferencedTypes(INamedTypeSymbol aggregate)
        {
            return aggregate
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(property => !property.IsImplicitlyDeclared)
                .Select(property => property.Type)
                .GroupBy(property => property.ToDisplayString())
                .Select(group => group.First());
        }

        private static bool IsIdentityAttribute(INamedTypeSymbol symbol)
        {
            return symbol?.Name == "IdentityAttribute" || symbol?.ToDisplayString() == "Muify.Domain.IdentityAttribute";
        }
    }
}