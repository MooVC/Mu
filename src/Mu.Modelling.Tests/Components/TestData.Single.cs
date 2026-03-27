namespace Mu.Modelling.Components;

using System.Diagnostics.CodeAnalysis;
using Kind = Mu.Modelling.Feature.Kind;

internal static partial class TestData
{
    public static partial class Single
    {
        public static readonly Model.Graph.Areas Areas;
        public static readonly Model.Graph.Areas.Area Mechanics;
        public static readonly Model.Graph.Areas.Area.Units Units;
        public static readonly Model.Graph.Areas.Area.Units.Unit Car;
        public static readonly Model Model;

        [SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline", Justification = "Order of initialization is required.")]
        static Single()
        {
            Model = new()
            {
                Areas =
                [
                    new()
                    {
                        Description = "Represents a Mechanics Shop",
                        Name = "Mechanics",
                        Units =
                        [
                            new Unit
                            {
                                Attributes =
                                [
                                    new() { Description = "The Number of Passenger Doors", Name = "Doors", Type = typeof(byte) },
                                    new() { Description = "The Manufacturer of the Car", Name = "Make", Type = typeof(string) },
                                    new() { Description = "The Name Ascribed to the Car by the Manufacturer", Name = "Model", Type = typeof(string) },
                                    new() { Description = "The Wheels Attached to the Car", Name = "Wheels", Type = (Name: "Wheel", IsArray: true, Qualifier: "MooVC.Testing.Mechanics.Car") },
                                ],
                                Components =
                                [
                                    new()
                                    {
                                        Identifier = new()
                                        {
                                            Name = "Location",
                                            Type = (Name: "Location", Qualifier: "MooVC.Testing.Mechanics.Car"),
                                        },
                                        Name = "Wheel",
                                    },
                                ],
                                Description = "Represents a Vehicle that has utilizes the services of the Mechanics",
                                Features =
                                [
                                    new()
                                    {
                                        Name = "Register",
                                        Parameters =
                                        [
                                            new() { Name = "Doors", Type = typeof(byte) },
                                            new() { Name = "Make", Type = typeof(string) },
                                            new() { Name = "Model", Type = typeof(string) },
                                        ],
                                        Mutational = new Mutational { Fact = "Registered", Type = Mutational.Kind.Creational },
                                        Type = Kind.Mutational,
                                    },
                                ],
                                Name = "Car",
                            },
                        ],
                    },
                ],
                Company = "MooVC",
                Description = "Test Model",
                Name = "Testing",
            };

            Areas = new(Model, Model.Areas);
            Mechanics = new(Areas, 0, Model, Model.Areas[0]);
            Units = new(Mechanics, Model, Mechanics.Value.Units);
            Car = new(Units, 0, Model, Units.Value[0]);
        }
    }
}