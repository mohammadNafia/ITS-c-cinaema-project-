namespace CinemaApp.Models;

public sealed record Room(string Name, int Capacity)
{
    public override string ToString() => $"{Name} (Capacity: {Capacity})";
}
