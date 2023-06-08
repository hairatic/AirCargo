using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AirCargo.CLI;
using AirCargo.Models;
using AirCargo.Infrastructure;
using AirCargo.Services;
using CommandLine;

namespace AirCargo
{
    public class AirCargoApplication
    {
        private readonly IAirCargoService _airCargoService;
        private readonly IOutput _output;

        public AirCargoApplication(IAirCargoService airCargoService, IOutput output)
        {
            _airCargoService = airCargoService;
            _output = output;
        }

        public async Task RunAsync(string[] args)
        {
            var result = await Parser.Default
                .ParseArguments<AddOrdersVerbOptions, AddFlightVerbOptions, ShowFlightsVerbOptions>(args)
                .MapResult((AddOrdersVerbOptions opt) => RunOrders(opt),
                    (AddFlightVerbOptions opt) => RunScheduleFlights(opt),
                    (ShowFlightsVerbOptions opt) => RunShowFlights(opt),
                    err => Task.FromResult(0));
        }

        async Task<int> RunShowFlights(ShowFlightsVerbOptions o)
        {
            var result = await _airCargoService.GetAllFlightsAsync();
            DisplayResult<List<Flight>>(result);
            return 0;
        }

        async Task<int> RunScheduleFlights(AddFlightVerbOptions o)
        {
            var result = await _airCargoService.ScheduleFlightsAsync(o.Filename);
            DisplayResult<bool>(result);
            return 0;
        }

        async Task<int> RunOrders(AddOrdersVerbOptions o)
        {
            var result = await _airCargoService.DispatchOrdersAsync(o.Filename);
            DisplayResult<List<Order>>(result);
            return 0;
        }

        void DisplayResult<T>(Result<T> result)
        {
            if (result.IsSuccessful)
            {
                if(result.Value is bool)
                    _output.WriteLine("Success");
                else
                {
                    string output = FormatJsonArrayByLines(JsonSerializer.Serialize(result.Value));
                    _output.WriteLine(output);
                }
            }
            else
            {
                string output = string.Join(Environment.NewLine, result.Errors);
                _output.WriteLine(output);
            }
        }

        string FormatJsonArrayByLines(string json)
        {
            return json.Replace("},", "}," + Environment.NewLine);
        }
    }
}
