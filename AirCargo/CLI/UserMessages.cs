using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirCargo.CLI
{
    public static class UserMessages
    {
        public const string FlightsFormatMsg = "Flights are expected in the following JSON format: \n" +
                                                      "[ \n" +
                                                      "  { \"flightNumber\": 1, \"departure\":\"XXX\", \"arrival\":\"YYY\", \"day\": 1 }, \n" +
                                                      "  { \"flightNumber\": 2, \"departure\":\"YYY\", \"arrival\":\"XXX\", \"day\": 2 } \n" +
                                                       "] \n" +
                                                      ", where XXX and YYY are airport codes";

        public const string EmptyInputMsg = "Input can not be empty";
        public const string UnavailableAirportsMsg = "One or more airport codes provided are invalid or out of service";
        public const string FormatError = "Input format error. Failed to parse";
        public const string NotEnoughPlanes = "Aircraft fleet capacity is not enough to schedule these flights";
        public const string FileNotFound = "Specified file can not be found";
        public const string FlightNumberExists = "Flight with this flightNumber already existis in the schedule";
        public const string OrderNumberExists = "Some order with the same order number already exist in the repository";
        public const string FlightDepartureSameAsArrival = "Flight departure and arrival airports should be different";
        public const string RepositoryIsBusy = "Repository is busy with another process";
        public const string RepositoryError = "Repository error.";
        public const string RepositoryCorrupted = "Repository data is corrupted";
    }
}
