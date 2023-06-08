using AirCargo.Infrastructure;
using AirCargo.Models;

namespace AirCargo.Repositories;

public interface IOrdersRepository
{
    Task<Result<List<Order>>> GetOrdersAsync();

    Task<Result<bool>> OverwriteOrdersAsync(List<Order> orders);
}