using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirCargo.Models;
using AirCargo.Infrastructure;

namespace AirCargo.Repositories
{
    public interface IFlightsRepository
    {
        Task<Result<List<Flight>>> GetFlightsAsync();

        Task<Result<bool>> OverwriteFlightsAsync(List<Flight> flights);
    }
}
