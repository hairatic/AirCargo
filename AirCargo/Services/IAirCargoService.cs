using AirCargo.Models;
using AirCargo.Infrastructure;

namespace AirCargo.Services;

public interface IAirCargoService
{
    Task<Result<List<Flight>>> GetAllFlightsAsync();

    Task<Result<bool>> ScheduleFlightsAsync(string scheduleFilePath);

    Task<Result<List<Order>>> DispatchOrdersAsync(string ordersFilePath);
}