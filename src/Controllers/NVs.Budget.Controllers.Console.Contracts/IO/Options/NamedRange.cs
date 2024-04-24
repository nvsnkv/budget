namespace NVs.Budget.Controllers.Console.Contracts.IO.Options;

public record NamedRange(string Name, DateTime From, DateTime Till)
{
    public bool Accepts(DateTime value) => From <= value && value < Till;
}