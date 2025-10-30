using System;
using System.Collections.Generic;
using System.Linq;

namespace CinemaApp.Models;

public sealed class Show
{
    public Guid Id { get; } = Guid.NewGuid();
    public Movie Movie { get; }
    public Room Room { get; }
    public DateTime Start { get; }
    public DateTime End => Start + Movie.Duration;
    public decimal Price { get; }

    private readonly HashSet<int> _taken = new();

    public int AvailableSeats => Room.Capacity - _taken.Count;

    public Show(Movie movie, Room room, DateTime start, decimal price)
    {
        Movie = movie ?? throw new ArgumentNullException(nameof(movie));
        Room = room ?? throw new ArgumentNullException(nameof(room));
        if (price < 0) throw new ArgumentException("Price can't be negative");

        Start = start;
        Price = price;
    }
    
    public bool TryBook(params int[] seats)
    {
        if (seats == null || seats.Length == 0) 
            return false;

        foreach (var seat in seats)
        {
            if (seat < 1 || seat > Room.Capacity)
                return false;
            if (_taken.Contains(seat))
                return false;
        }

        foreach (var seat in seats)
        {
            _taken.Add(seat);
        }

        return true;
    }

    public override string ToString()
    {
        return $"[{Id:N}] {Movie.Title} | {Room.Name} | {Start:yyyy-MM-dd HH:mm} - {End:HH:mm} | ${Price:F2} | Seats: {AvailableSeats}/{Room.Capacity} available";
    }
}
