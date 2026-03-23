namespace Mu.Modelling.Components;

using Kind = Mu.Modelling.Feature.Kind;

internal static class TestData
{
    public static readonly Model Single = new()
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
}