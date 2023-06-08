using AirCargo.CLI;
using AirCargo.Models;
using AirCargo.Repositories;
using AirCargo.Infrastructure;
using AirCargo.Validators;
using Microsoft.Extensions.Options;

namespace AirCargo.Services;

public class AirCargoService : IAirCargoService
{
    private readonly int _airFleetCapacity = 3;
    private readonly int _defaultPlaneCapacity = 20;

    private readonly FlightValidator _flightValidator;
    private readonly OrdersValidator _orderValidator;
    private readonly IFlightsRepository _flightRepository;
    private readonly IOrdersRepository _orderRepository;

    public AirCargoService(FlightValidator flightValidator, OrdersValidator orderValidator, 
                           IFlightsRepository flightRepository, IOrdersRepository orderRepository,
                           IOptions<FleetOptions> fleetOptions)
    {
        _flightValidator = flightValidator;
        _orderValidator = orderValidator;
        _flightRepository = flightRepository;
        _orderRepository = orderRepository;

        _airFleetCapacity = fleetOptions.Value.PlanesAvailable;
        _defaultPlaneCapacity = fleetOptions.Value.DefaultPlaneCapacity;
    }

    public async Task<Result<List<Order>>> DispatchOrdersAsync(string ordersFilePath)
    {
        var result = new Result<List<Order>>();
        result.IsSuccessful = true;

        if (!File.Exists(ordersFilePath))
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.FileNotFound);
            return result;
        }

        string content = await File.ReadAllTextAsync(ordersFilePath);
        var validationResult = _orderValidator.ValidateAndDeserialize(content);

        if (!validationResult.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(validationResult.Errors);
            return result;
        }

        List<Order> newOrders = validationResult.Value;

        var ordersResponse = await _orderRepository.GetOrdersAsync();
        if (!ordersResponse.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(ordersResponse.Errors);
            return result;
        }

        List<Order> repoOrders = ordersResponse.Value;


        var flightsResponse = await _flightRepository.GetFlightsAsync();
        if (!flightsResponse.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(flightsResponse.Errors);
            return result;
        }

        List<Flight> repoFlights = flightsResponse.Value;

        var flightOrderData = GetOrdersByFlighAndUniqueNumbers(repoOrders);
        HashSet<int> orderUniqueNumbers = flightOrderData.Item1;
        Dictionary<int, List<Order>> ordersByFlights = flightOrderData.Item2;

        foreach (var newOrder in newOrders)
        {
            if (orderUniqueNumbers.Contains(newOrder.OrderPriority))
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.OrderNumberExists);
                return result;
            }

            bool flightFound = false;

            //flights are sorted by Day
            for (int i = 0; i < repoFlights.Count; i++)
            {
                if (IsFlightApplicable(repoFlights[i], newOrder) &&
                    FlightHasCapacity(repoFlights[i], ordersByFlights))
                {
                    newOrder.Day = repoFlights[i].Day;
                    newOrder.FlightNumber = repoFlights[i].FlightNumber;

                    if(!ordersByFlights.ContainsKey(repoFlights[i].FlightNumber))
                        ordersByFlights.Add(repoFlights[i].FlightNumber, new List<Order>());

                    ordersByFlights[repoFlights[i].FlightNumber].Add(newOrder);
                    repoOrders.Add(newOrder);
                    orderUniqueNumbers.Add(newOrder.OrderPriority);

                    flightFound = true;
                    break;
                }
            }

            if (!flightFound)
            {
                newOrder.FlightNumber = -1;
                newOrder.Day = -1;
            }
        }

        await _orderRepository.OverwriteOrdersAsync(repoOrders);

        result.Value = GroupByAndSortOrders(newOrders);
        return result;
    }


    // orders that have not been dispatched grooped in the bottom
    // other orders are sorted by order number
    List<Order> GroupByAndSortOrders(List<Order> orders)
    {
        List<Order> sorted = new List<Order>();

        var grouped = orders.GroupBy(o => IsDispatched(o))
                                .Select(group =>
                                new
                                {
                                    Name = group.Key ? "Dispatched" : "Not Dispatched",
                                    Orders = group.OrderBy(x => x.OrderPriority)
                                }).ToList();

        foreach (var g in grouped)
            sorted.AddRange(g.Orders.ToList());

        return sorted;
    }

    static bool IsDispatched(Order o)
    {
        return o.FlightNumber != -1;
    }

    static bool IsFlightApplicable(Flight flignt, Order newOrder)
    {
        return flignt.Arrival == newOrder.Destination &&
               flignt.Departure == newOrder.Origin;
    }

    bool FlightHasCapacity(Flight flignt, Dictionary<int, List<Order>> ordersByFlights)
    {
        return !ordersByFlights.ContainsKey(flignt.FlightNumber) ||
               ordersByFlights[flignt.FlightNumber].Count < _defaultPlaneCapacity;
    }


    // utilize memory to reduce lookup time
    (HashSet<int>, Dictionary<int, List<Order>>) GetOrdersByFlighAndUniqueNumbers(List<Order> orders)
    {
        var map = new Dictionary<int, List<Order>>();
        var orderNums = new HashSet<int>();

        foreach (var o in orders)
        {
            if(!map.ContainsKey(o.FlightNumber))
                map.Add(o.FlightNumber, new List<Order>());

            map[o.FlightNumber].Add(o);

            if (!orderNums.Contains(o.OrderPriority))
                orderNums.Add(o.OrderPriority);
        }

        return (orderNums, map);
    }

    public async Task<Result<List<Flight>>> GetAllFlightsAsync()
    {
        var repoFlightsResponse = await _flightRepository.GetFlightsAsync();
        var result = new Result<List<Flight>>();
        result.IsSuccessful = true;

        if (!repoFlightsResponse.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(repoFlightsResponse.Errors);
            return result;
        }

        result.Value = repoFlightsResponse.Value;
        return result;
    }

    public async Task<Result<bool>> ScheduleFlightsAsync(string scheduleFilePath)
    {
        Result<bool> result = new Result<bool>();

        if (!File.Exists(scheduleFilePath))
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.FileNotFound);
            return result;
        }

        string content = await File.ReadAllTextAsync(scheduleFilePath);
        Result<List<Flight>> newFlights = _flightValidator.ValidateAndDeserialize(content);

        if (!newFlights.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(newFlights.Errors);
            return result;
        }

        var repoResponse = await _flightRepository.GetFlightsAsync();
        if (!repoResponse.IsSuccessful)
        {
            result.IsSuccessful = false;
            result.Errors.AddRange(repoResponse.Errors);
            return result;
        }

        List<Flight> repoFlights = repoResponse.Value;

        var flightData = GetFlightsCountAndUniqueNumbers(repoFlights);
        HashSet<int> flightsNumbers = flightData.Item1;
        Dictionary<int, int> flightsCountPerDay = flightData.Item2;

        foreach (var newFlight in newFlights.Value)
        {
            if (flightsNumbers.Contains(newFlight.FlightNumber))
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.FlightNumberExists);
                return result;
            }

            if (flightsCountPerDay.ContainsKey(newFlight.Day) &&
                flightsCountPerDay[newFlight.Day] >= _airFleetCapacity)
            {
                result.IsSuccessful = false;
                result.Errors.Add(UserMessages.NotEnoughPlanes);
                return result;
            }

            repoFlights.Add(newFlight);

            if(!flightsCountPerDay.ContainsKey(newFlight.Day))
                flightsCountPerDay.Add(newFlight.Day, 0);

            flightsCountPerDay[newFlight.Day] += 1;
        }

        await _flightRepository.OverwriteFlightsAsync(repoFlights);

        result.IsSuccessful = true;
        result.Value = true;
        return result;
    }

    // more memory, but better time complexity
    (HashSet<int>, Dictionary<int, int>) GetFlightsCountAndUniqueNumbers(List<Flight> flights)
    {
        var occurenciesMap = new Dictionary<int, int>();
        var flightNumbers = new HashSet<int>();

        foreach (var f in flights)
        {
            if(!occurenciesMap.ContainsKey(f.Day))
                occurenciesMap.Add(f.Day, 0);

            occurenciesMap[f.Day] += 1;
            flightNumbers.Add(f.FlightNumber);
        }

        return (flightNumbers, occurenciesMap);
    }


}