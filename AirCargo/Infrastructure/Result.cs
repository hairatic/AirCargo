namespace AirCargo.Infrastructure;

public class Result<T>
{
    public bool IsSuccessful { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public T Value { get; set; }
}