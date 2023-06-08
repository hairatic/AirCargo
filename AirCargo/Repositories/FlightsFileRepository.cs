using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using AirCargo.CLI;
using AirCargo.Models;
using AirCargo.Infrastructure;
using AirCargo.Services;

namespace AirCargo.Repositories
{
    public class FlightsFileRepository : IFlightsRepository
    {
        private readonly FileAccessService _fileAcess;
        private const string FlightRepoName = "flights.txt";

        public FlightsFileRepository(FileAccessService fileAcess)
        {
            _fileAcess = fileAcess;
        }

        public async Task<Result<List<Flight>>> GetFlightsAsync()
        {
            var result = new Result<List<Flight>>();
            result.IsSuccessful = true;

            var contentResponse = await _fileAcess.ReadFileAsync(FlightRepoName);
            if (!contentResponse.IsSuccessful)
            {
                result.IsSuccessful = false;
                result.Errors.AddRange(contentResponse.Errors);
                return result;
            }

            if (string.IsNullOrWhiteSpace(contentResponse.Value))
            {
                result.Value = new List<Flight>();
                return result;
            }

            var repoFlights = new List<Flight>();

            try
            {
                repoFlights = JsonSerializer.Deserialize<List<Flight>>(contentResponse.Value);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.RepositoryCorrupted);
                result.Errors.Add(Environment.NewLine + ex.Message);
                return result;
            }

            result.Value = repoFlights;
            return result;
        }

        public async Task<Result<bool>> OverwriteFlightsAsync(List<Flight> flights)
        {
            var result = new Result<bool>();
            result.IsSuccessful = true;

            var sortedFlights = flights.OrderBy(x => x.Day).ToList();
            string updatedContent = FormatJsonArrayByLines(JsonSerializer.Serialize(sortedFlights));
            var operationResponse = await _fileAcess.WriteFileAsync(FlightRepoName, updatedContent);

            if (!operationResponse.IsSuccessful)
            {
                result.IsSuccessful = false;
                result.Errors.AddRange(operationResponse.Errors);
                return result;
            }

            result.Value = true;
            return result;
        }

        string FormatJsonArrayByLines(string json)
        {
            return json.Replace("},", "}," + Environment.NewLine);
        }
    }
}
