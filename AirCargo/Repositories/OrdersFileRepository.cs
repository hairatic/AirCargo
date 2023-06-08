using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AirCargo.CLI;
using AirCargo.Infrastructure;
using AirCargo.Models;
using AirCargo.Services;

namespace AirCargo.Repositories
{
    public class OrdersFileRepository : IOrdersRepository
    {
        private readonly FileAccessService _fileAcess;
        private const string FlightRepoName = "orders.txt";

        public OrdersFileRepository(FileAccessService fileAcess)
        {
            _fileAcess = fileAcess;
        }

        public async Task<Result<bool>> OverwriteOrdersAsync(List<Order> orders)
        {
            var result = new Result<bool>();
            result.IsSuccessful = true;

            var sortedOrders = orders.OrderBy(x => x.Day).ToList();
            string updatedContent = FormatJsonArrayByLines(JsonSerializer.Serialize(sortedOrders));
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

        public async Task<Result<List<Order>>> GetOrdersAsync()
        {
            var result = new Result<List<Order>>();
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
                result.Value = new List<Order>();
                return result;
            }

            var repoOrders = new List<Order>();

            try
            {
                repoOrders = JsonSerializer.Deserialize<List<Order>>(contentResponse.Value);
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.RepositoryCorrupted);
                result.Errors.Add(Environment.NewLine + ex.Message);
                return result;
            }

            result.Value = repoOrders;
            return result;
        }
        string FormatJsonArrayByLines(string json)
        {
            return json.Replace("},", "}," + Environment.NewLine);
        }
    }
}
