using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AirCargo.Models
{
    public record Flight
    {
        [JsonPropertyName("departure")]
        public string Departure { get; init; }

        [JsonPropertyName("arrival")]
        public string Arrival { get; init; }

        [JsonPropertyName("day")]
        public int Day { get; init; }

        [JsonPropertyName("flightNumber")]
        public int FlightNumber { get; init; }
    }
}
