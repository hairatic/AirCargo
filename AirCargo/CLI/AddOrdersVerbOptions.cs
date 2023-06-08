using CommandLine;

namespace AirCargo.CLI;

[Verb("add-orders", HelpText = "Import orders and assign them to cargo flights")]
public class AddOrdersVerbOptions
{
    [Option("filename", Required = true, HelpText = "Path to JSON with order list")]
    public string Filename { get; init; }
}
