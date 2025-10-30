namespace CinemaApp.Models;

public sealed record Movie(string Title, TimeSpan Duration)
{
    public override string ToString() => $"{Title} ({Duration.Hours}h {Duration.Minutes}m)";
}
