namespace Muify
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using MooVC;
    using MooVC.Syntax;
    using Mu.Modelling;
    using Muify.Domain;
    using Muify.Modelling;
    using Muify.Syntax.CSharp;
    using Attribute = Mu.Modelling.Attribute;

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

            const int MinimumSegments = 4;

            if (segments.Length < MinimumSegments)
            {
                return Model.Undefined;
            }

            Name company = segments[0];
            Name name = segments[1];

            Model model = Model.Undefined
                .For(company)
                .Named(name);

            const int DomainSegments = 4;

            if (segments.Length < DomainSegments)
            {
                return model;
            }

            Feature feature = Feature.Undefined;

            const int FeatureSegments = 5;

            if (segments.Length == FeatureSegments)
            {
                feature = GetFeatureModel(compilation, (Area: segments[2], Feature: segments[4], Unit: segments[3]));
            }

            return GetDomainModel(compilation, feature, (Area: segments[2], Unit: segments[3]), cancellationToken);
        }

        private static Model GetDomainModel(Compilation compilation, Feature feature, (Name Area, Name Unit) names, CancellationToken cancellationToken)
        {
            INamedTypeSymbol definition = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Unit}");

            if (definition is null || !definition.IsRecord)
            {
                return Model.Undefined;
            }

            IdentifyMembers(definition, out Component[] components, out List[] lists, cancellationToken);

            return Model.Undefined
                .Defines(area => area
                    .Named(names.Area)
                    .ResponsibleFor(unit => unit
                        .Named(names.Unit)
                        .Featuring(feature)
                        .Owns(components)
                        .Sets(lists)));
        }

        private static Feature GetFeatureModel(Compilation compilation, (Name Area, Name Feature, Name Unit) names)
        {
            INamedTypeSymbol request = compilation.GetTypeByMetadataName($"{compilation.AssemblyName}.{names.Feature}");

            if (request is null)
            {
                return Feature.Undefined;
            }

            IPropertySymbol[] results = GetResults(request);

            return Feature.Undefined
                .Named(names.Feature)
                .Enumerate(CreateResult, results);
        }

        private static Feature CreateResult(IPropertySymbol result, Feature feature)
        {
            return feature.Returning(member => member
                .Named(result.Name)
                .OfType(result.Type));
        }

        private static IPropertySymbol[] GetResults(INamedTypeSymbol request)
        {
            INamedTypeSymbol result = request
                .GetTypeMembers()
                .FirstOrDefault(type => type.IsRecord && type.Name == nameof(Result));

            if (result is null)
            {
                return Array.Empty<IPropertySymbol>();
            }

            return result
                .GetMembers()
                .OfType<IPropertySymbol>()
                .ToArray();
        }

        private static void IdentifyMembers(INamedTypeSymbol definition, out Component[] components, out List[] lists, CancellationToken cancellationToken)
        {
            var entities = new List<Component>();
            var enumerations = new List<List>();
            var values = new List<Component>();

            foreach (ITypeSymbol type in GetReferencedTypes(definition))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!(type is INamedTypeSymbol named && named.SpecialType == SpecialType.None))
                {
                    continue;
                }

                if (named.IsRecord)
                {
                    values.Add(CatalogValue(named));

                    continue;
                }

                if (!(named.TypeKind == TypeKind.Class || named.TypeKind == TypeKind.Struct))
                {
                    continue;
                }

                entities.Add(CatalogEntity(named));
            }

            components = entities
                .Concat(values)
                .ToArray();

            lists = enumerations.ToArray();
        }

        private static Component CatalogEntity(INamedTypeSymbol entity)
        {
            IPropertySymbol[] properties = GetProperties(entity);
            Attribute identity = GetIdentity(properties, out IPropertySymbol match);

            return Component.Undefined
                .AttributedWith(properties.Except(new[] { match }))
                .IdentifiedBy(identity)
                .Named(entity.Name);
        }

        private static Component CatalogValue(INamedTypeSymbol value)
        {
            IPropertySymbol[] properties = GetProperties(value);

            return Component.Undefined
                .AttributedWith(properties)
                .Named(value.Name);
        }

        private static Attribute GetIdentity(IPropertySymbol[] properties, out IPropertySymbol identity)
        {
            identity = properties.FirstOrDefault(property => property
                .GetAttributes()
                .Any(attribute => IsIdentityAttribute(attribute.AttributeClass)));

            if (identity is null)
            {
                return Attribute.Undefined;
            }

            return Attribute.Undefined.From(identity);
        }

        private static IPropertySymbol[] GetProperties(INamedTypeSymbol symbol)
        {
            return GetProperties(symbol, property => property.SetMethod is object);
        }

        private static IPropertySymbol[] GetProperties(INamedTypeSymbol symbol, Func<IPropertySymbol, bool> predicate)
        {
            return symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(predicate)
                .ToArray();
        }

        private static IEnumerable<ITypeSymbol> GetReferencedTypes(INamedTypeSymbol unit)
        {
            return GetProperties(unit)
                .Select(property => property.Type)
                .GroupBy(property => property.ToDisplayString())
                .Select(group => group.First());
        }

        private static bool IsIdentityAttribute(INamedTypeSymbol symbol)
        {
            return symbol is object
                && (symbol.Name == $"{IdentityAttributeStrategy.Name}Attribute"
                 || symbol.ToDisplayString() == $"Muify.Domain.{IdentityAttributeStrategy.Name}Attribute");
        }
    }
}