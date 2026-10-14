namespace Muify.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using Mu.Modelling;
    using Muify.Service;
    using Muify.Syntax.CSharp;

    internal sealed class GenerateBinderWhenSharedAssemblyVisited
        : IModelVisitor<Model.Graph.Metadata, File>
    {
        internal const string BinderName = "Binder";

        public static Snippet GetApplicationRegistrations(IEnumerable<Qualification> binders)
        {
            string[] registrations = binders
                .Distinct()
                .OrderBy(binder => Configuration.Render((Symbol)binder), StringComparer.Ordinal)
                .Select(binder => $"_ = global::Mu.Serialization.RuntimeTypeModelExtensions.BindOnce<{Configuration.Render((Symbol)binder)}>(global::ProtoBuf.Meta.RuntimeTypeModel.Default);")
                .ToArray();

            return registrations.Length == 0
                ? Snippet.Empty
                : Snippet.From(Configuration.Options, registrations);
        }

        public IEnumerable<File> Observe(Model.Graph.Metadata metadata)
        {
            if (!metadata._Value.IsSharedProject || metadata._Value.HasBinder)
            {
                yield break;
            }

            Symbol[] types = metadata._Value.SharedTypes
                .Where(CanBindSharedType)
                .Select(type => (Symbol)type.Qualification)
                .Distinct()
                .OrderBy(Configuration.Render, StringComparer.Ordinal)
                .ToArray();

            if (types.Length == 0)
            {
                yield break;
            }

            string[] registrations = types.Select(type => Snippet.From(
                Configuration.Options,
                $"_ = {Configuration.Render(type)}.Bind(model);")
                .Block(Configuration.Options, $"if (!global::Mu.Serialization.RuntimeTypeModelExtensions.HasBinding<{Configuration.Render(type)}>(model))")
                .ToString()).ToArray();
            Snippet bindings = Snippet.From(Configuration.Options, registrations)
                .Append(Configuration.Options, Snippet.Blank)
                .Append(Configuration.Options, "return model;");

            string content = Builder.New<Definition>()
                .For<Class>(binder => binder
                    .WithBinder(bindings, BinderName)
                    .WithExtensibility(Modifiers.Sealed)
                    .WithScope(Scopes.Public))
                .From(metadata._Value.ApplicationNamespace)
                .ToSnippet(Configuration.Options);

            yield return new File(content, $"{metadata._Value.ApplicationNamespace}.{BinderName}");
        }

        private static bool CanBindSharedType(Poco type)
        {
            return type.IsAccessible
                && !type.Qualification.IsUnnamed
                && (type.HasBinder || GenerateBinderWhenFeatureReferenceVisited.CanGenerateBinder(type));
        }
    }
}