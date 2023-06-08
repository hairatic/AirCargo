using System.Text.Json;
using AirCargo.CLI;
using AirCargo.Models;
using AirCargo.Infrastructure;

namespace AirCargo.Validators;

public class FlightValidator
{
    private readonly HashSet<string> _availableAirports;

    public FlightValidator(List<string> availableAirports)
    {
        _availableAirports = new HashSet<string>(availableAirports);
    }

    public Result<List<Flight>> ValidateAndDeserialize(string content)
    {
        var result = new Result<List<Flight>>();

        if (string.IsNullOrWhiteSpace(content))
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.EmptyInputMsg);
            return result;
        }

        try
        {
            List<Flight> flights = JsonSerializer.Deserialize<List<Flight>>(content);

            foreach (var f in flights)
            {
                if (!IsAirportsInService(f))
                {
                    result.IsSuccessful = false;
                    result.Errors.Add(UserMessages.UnavailableAirportsMsg);
                    return result;
                }

                if (!IsDepartureAndArrivalDifferent(f))
                {
                    result.IsSuccessful = false;
                    result.Errors.Add(UserMessages.FlightDepartureSameAsArrival);
                    return result;
                }
            }

            result.IsSuccessful = true;
            result.Value = flights;
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.FlightsFormatMsg);
            result.Errors.Add(Environment.NewLine + ex.Message);
        }

        return result;
    }

    bool IsAirportsInService(Flight f)
    {
        if (!_availableAirports.Contains(f.Arrival))
            return false;

        if (!_availableAirports.Contains(f.Departure))
            return false;

        return true;
    }

    bool IsDepartureAndArrivalDifferent(Flight f)
    {
        return f.Arrival != f.Departure;
    }

}

