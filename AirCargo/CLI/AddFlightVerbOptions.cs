using CommandLine;

namespace AirCargo.CLI;

[Verb("add-flights", HelpText = "Import scheduled flights with JSON file")]
public class AddFlightVerbOptions
{
    [Option("filename", Required = true, HelpText = "Path to JSON with flight schedule")]
    public string Filename { get; init; }
}
