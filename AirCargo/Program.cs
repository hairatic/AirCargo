using System.Text.Json;
using System.Text.Json.Nodes;
using AirCargo.CLI;
using AirCargo.Infrastructure;
using AirCargo.Models;
using AirCargo.Repositories;
using AirCargo.Services;
using AirCargo.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace AirCargo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
             var configuration = BuildConfiguration();
             var serviceProvider = BuildServiceProvider(configuration);
 
             var app = serviceProvider.GetRequiredService<AirCargoApplication>();
             await app.RunAsync(args);
        }
        private static ServiceProvider BuildServiceProvider(IConfigurationRoot configuration)
        {
            var services = new ServiceCollection();
            ConfigureServices(configuration, services);

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static void ConfigureServices(IConfigurationRoot configuration, IServiceCollection services)
        {
            var airports = configuration.GetSection("AirCargoSettings:AvailableAirports").Get<List<string>>();

            services.Configure<FleetOptions>(options => configuration.GetSection("AirCargoSettings:FleetConfiguration").Bind(options));

            services.AddSingleton<FlightValidator>(x => new FlightValidator(airports));
            services.AddSingleton<OrdersValidator>();

            services.AddSingleton<IOutput, ConsoleOutput>();
            services.AddSingleton<FileAccessService>();
            services.AddSingleton<IFlightsRepository, FlightsFileRepository>();
            services.AddSingleton<IOrdersRepository, OrdersFileRepository>();
            services.AddSingleton<IAirCargoService, AirCargoService>();
            services.AddSingleton<AirCargoApplication>();
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            return configuration;
        }
    }
}

