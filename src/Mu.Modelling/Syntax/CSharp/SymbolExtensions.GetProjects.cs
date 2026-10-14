namespace Mu.Modelling.Syntax.CSharp
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Ardalis.GuardClauses;
    using MooVC.Collections.Generic;
    using MooVC.Syntax;
    using MooVC.Syntax.CSharp;
    using static Mu.Modelling.Syntax.CSharp.SymbolExtensions_Resources;

    public static partial class SymbolExtensions
    {
        public static ImmutableArray<Qualifier> GetProjects(this IEnumerable<Symbol> symbols, Name company, params Name[] names)
        {
            return symbols.GetScopedProjects(company, Qualifier.Unqualified, names);
        }

        internal static ImmutableArray<Qualifier> GetScopedProjects(this IEnumerable<Symbol> symbols, Name company, Qualifier scope, params Name[] names)
        {
            _ = Guard.Against.Null(symbols, message: GetProjectsSymbolsRequired);
            _ = Guard.Against.Null(company, message: GetProjectsCompanyRequired);
            _ = Guard.Against.Null(scope, message: GetProjectsNamesRequired);
            _ = Guard.Against.Null(names, message: GetProjectsNamesRequired);

            if (!HasName(names, 0))
            {
                return ImmutableArray<Qualifier>.Empty;
            }

            ImmutableArray<Name> prefix = (company.IsUnnamed
                ? ImmutableArray.Create(names[0])
                : ImmutableArray.Create(company, names[0]))
                .AddRange(scope.ToImmutableArray());

            IEnumerable<Qualifier> unique = symbols
                .SelectMany(symbol => symbol)
                .Distinct()
                .Where(qualifier => qualifier.Take(prefix.Length).SequenceEqual(prefix));

            var projects = new HashSet<Qualifier>();

            GetProjects(prefix.Length, projects, names, unique);

            return projects.ToImmutableArray();
        }

        private static void GetProjects(int areaOffset, HashSet<Qualifier> projects, Name[] names, IEnumerable<Qualifier> model)
        {
            const int AreaIndex = 1;
            const int UnitIndex = 2;

            if (!HasName(names, AreaIndex))
            {
                AddProjects(projects, model, areaOffset + 1);
                return;
            }

            IGrouping<Name, Qualifier> area = FindGroup(model, areaOffset, names[AreaIndex]);

            if (area is null)
            {
                return;
            }

            int unitOffset = areaOffset + 1;

            if (!HasName(names, UnitIndex))
            {
                AddProjects(projects, area, unitOffset);

                return;
            }

            foreach (IGrouping<Name, Qualifier> unit in area.GroupBy(qualifier => qualifier[unitOffset]))
            {
                if (unit.Key == names[UnitIndex])
                {
                    continue;
                }

                AddProjects(projects, unit, unitOffset + 1);
            }
        }

        private static IGrouping<Name, Qualifier> FindGroup(IEnumerable<Qualifier> qualifiers, int offset, Name name)
        {
            return qualifiers
                .GroupBy(qualifier => qualifier[offset])
                .FirstOrDefault(group => group.Key == name);
        }

        private static void AddProjects(HashSet<Qualifier> projects, IEnumerable<Qualifier> qualifiers, int length)
        {
            projects.AddRange(qualifiers.Select(qualifier => new Qualifier(qualifier.Take(length).ToImmutableArray())));
        }

        private static bool HasName(Name[] names, int offset)
        {
            return names.Length > offset && !names[offset].IsUnnamed;
        }
    }
}