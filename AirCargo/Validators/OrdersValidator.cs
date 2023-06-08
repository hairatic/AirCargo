using AirCargo.CLI;
using AirCargo.Models;
using AirCargo.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AirCargo.Validators;

public class OrdersValidator
{
    public Result<List<Order>> ValidateAndDeserialize(string content)
    {
        var result = new Result<List<Order>>();

        if (string.IsNullOrWhiteSpace(content))
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.EmptyInputMsg);
            return result;
        }

        try
        {
            JObject root = JObject.Parse(content);
            List<Order> orders = new List<Order>();

            foreach (var item in root)
            {
                string orderName = item.Key;
                Order o = JsonConvert.DeserializeObject<Order>(item.Value.ToString()); 
                o.OrderName = orderName;

                if (!IsOrderNameValid(orderName))
                {
                    throw new FormatException("Order name format is not valid");
                }

                orders.Add(o);
            }

            result.IsSuccessful = true;
            result.Value = orders;
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.Errors.Add(UserMessages.FormatError);
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    bool IsOrderNameValid(string orderName)
    {
        int priority;

        return orderName.Contains("-") && 
               orderName.Split('-').Length == 2 &&
               Int32.TryParse(orderName.Split('-')[1], out priority);
    }
}