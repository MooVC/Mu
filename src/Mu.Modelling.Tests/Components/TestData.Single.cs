namespace Mu.Modelling.Components;

using System.Diagnostics.CodeAnalysis;
using MooVC.Syntax.CSharp;

internal static partial class TestData
{
    public static partial class Single
    {
        public static readonly Model.Graph.Areas Areas;
        public static readonly Model.Graph.Areas.Area Mechanics;
        public static readonly Model.Graph.Areas.Area.Units Units;
        public static readonly Model.Graph.Areas.Area.Units.Unit Car;
        public static readonly Model.Graph.Areas.Area.Units.Unit.Components Components;
        public static readonly Model.Graph.Areas.Area.Units.Unit.Components.Component Pressure;
        public static readonly Model.Graph.Areas.Area.Units.Unit.Components.Component Wheel;
        public static readonly Model.Graph.Areas.Area.Units.Unit.Lists Lists;
        public static readonly Model.Graph.Areas.Area.Units.Unit.Lists.List Location;
        public static readonly Model Model;

        [SuppressMessage("Minor Code Smell", "S3963:\"static\" fields should be initialized inline", Justification = "Order of initialization is required.")]
        static Single()
        {
            Model = new Model()
                .Defines(mechanics => mechanics
                    .DescribedAs("Represents a Mechanics Shop")
                    .Named("Mechanics")
                    .ResponsibleFor(car => car
                        .AttributedWith(doors => doors
                            .DescribedAs("The Number of Passenger Doors")
                            .Named("Doors")
                            .OfType(typeof(byte)))
                        .AttributedWith(make => make
                            .DescribedAs("The Manufacturer of the Car")
                            .Named("Make")
                            .OfType(typeof(string)))
                        .AttributedWith(model => model
                            .DescribedAs("The Name Ascribed to the Car by the Manufacturer")
                            .Named("Model")
                            .OfType(typeof(string)))
                        .AttributedWith(model => model
                            .DescribedAs("The Wheels Attached to the Car")
                            .Named("Wheels")
                            .OfType(type => type
                                .IsArray(true)
                                .Named((Moniker: "Wheel", Qualifier: "MooVC.Testing.Mechanics.Car"))))
                        .DescribedAs("Represents a Vehicle that has utilizes the services of the Mechanics")
                        .Featuring(register => register
                            .DescribedAs("Registers a Car within the Mechanics System")
                            .IsMutational(register => register
                                .OfType(Mutational.Kinds.Creational)
                                .Raises("Registered"))
                            .Named("Register")
                            .Using((Name: "Doors", Type: typeof(byte)))
                            .Using((Name: "Make", Type: typeof(string)))
                            .Using((Name: "Model", Type: typeof(string))))
                        .Named("Car")
                        .Owns(DefinePressure())
                        .Owns(DefineWheel())
                        .Sets(DefineLocations())))
                .DescribedAs("Test Model")
                .For("MooVC")
                .Named("Testing");

            Areas = new(Model, Model.Areas);
            Mechanics = new(Areas, 0, Model, Model.Areas[0]);
            Units = new(Mechanics, Model, Mechanics.Value.Units);
            Car = new(Units, 0, Model, Units.Value[0]);
            Components = new(Car, Model, Car.Value.Components);
            Pressure = new(Components, 0, Model, Components.Value[0]);
            Wheel = new(Components, 1, Model, Components.Value[1]);
            Lists = new(Car, Model, Car.Value.Lists);
            Location = new(Lists, 0, Model, Lists.Value[0]);
        }

        private static Func<List, List> DefineLocations()
        {
            return locations => locations
                .Containing((Description: "The Front Left Wheel", Name: "FrontLeft"))
                .Containing((Description: "The Front Right Wheel", Name: "FrontRight"))
                .Containing((Description: "The Rear Left Wheel", Name: "RearLeft"))
                .Containing((Description: "The Rear Right Wheel", Name: "RearRight"))
                .DescribedAs("Represents the Location of the Wheel on the Car")
                .Named("Locations");
        }

        private static Func<Component, Component> DefinePressure()
        {
            return pressure => pressure
                .AttributedWith(unit => unit
                    .DescribedAs("The Unit of Measurement Associated with the Pressure")
                    .Named("Unit")
                    .OfType(type => type
                        .IsArray(true)
                        .Named((Moniker: "Unit", Qualifier: "MooVC.Testing.Mechanics.Car"))))
                .AttributedWith(value => value
                    .DescribedAs("The Value Associated with the Pressure based on the Unit")
                    .Named("Value")
                    .OfType(typeof(decimal)))
                .DescribedAs("Represents a Pressure Measurement Associated with a Wheel")
                .Named("Pressure");
        }

        private static Func<Component, Component> DefineWheel()
        {
            return wheel => wheel
                .DescribedAs("Represents a Wheel Attached to the Car")
                .AttributedWith(pressure => pressure
                    .DescribedAs("The Pressure of the Tyre on the Wheel")
                    .Named("Pressure")
                    .OfType((Name: "Pressure", Qualifier: "MooVC.Testing.Mechanics.Car")))
                .IdentifiedBy(location => location
                    .DescribedAs("The Location of the Wheel on the Car")
                    .Named("Location")
                    .OfType((Name: "Location", Qualifier: "MooVC.Testing.Mechanics.Car")))
                .Named("Wheel");
        }
    }
}