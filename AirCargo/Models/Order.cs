using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AirCargo.Models
{
    public class Order
    {
        [JsonPropertyName("order")]
        public string OrderName { get; set; }

        [JsonPropertyName("destination")]
        public string Destination { get; init; }

        [JsonPropertyName("origin")]
        public string Origin { get; init; } = "YUL";

        [JsonPropertyName("flightNumber")]
        public int FlightNumber { get; set; }

        [JsonPropertyName("day")]
        public int Day { get; set; }

        [JsonIgnore]
        public int OrderPriority
        {
            get
            {
                try {
                    return Int32.Parse(OrderName.Split("-")[1]);
                }
                catch { }

                return 0;
            }
        }
    }
}
