using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Models;
using CinemaApp.Services;

namespace CinemaApp;

public static class App
{
    private static readonly Dictionary<string, Room> rooms = new();
    private static readonly Dictionary<string, Movie> movies = new();
    private static readonly IShowStore store = new InMemoryShowStore();
    private static readonly IMarathonPlanner planner = new GreedyMarathonPlanner();

    public static void Run()
    {
        ShowWelcome();
        ShowMenu();

        while (true)
        {
            Console.Write("\n> ");
            var line = Console.ReadLine();
            if (line == null) break;

            line = line.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (int.TryParse(line, out var choice))
            {
                ProcessMenuChoice(choice);
                continue;
            }

            var parts = Split(line);
            if (parts.Length == 0) continue;

            var cmd = parts[0].ToLowerInvariant();
            if (cmd == "exit" || cmd == "0")
            {
                Console.WriteLine("\nGoodbye");
                break;
            }

            try
            {
                ExecuteCommand(cmd, parts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }
    }

    private static void ProcessMenuChoice(int choice)
    {
        switch (choice)
        {
            case 0:
                Console.WriteLine("\nGoodbye!");
                Environment.Exit(0);
                break;
            case 1:
                AddRoomInteractive();
                break;
            case 2:
                AddMovieInteractive();
                break;
            case 3:
                AddShowInteractive();
                break;
            case 4:
                ListShowsInteractive();
                break;
            case 5:
                BookInteractive();
                break;
            case 6:
                MarathonInteractive();
                break;
            default:
                Console.WriteLine($"\nInvalid choice: {choice}. Enter 0-6 or type 'menu'.");
                break;
        }
    }

    private static void ExecuteCommand(string cmd, string[] parts)
    {
        switch (cmd)
        {
            case "help":
            case "menu":
                ShowMenu();
                break;
            case "add-room":
            case "1":
                HandleAddRoom(parts);
                break;
            case "add-movie":
            case "2":
                HandleAddMovie(parts);
                break;
            case "add-show":
            case "3":
                HandleAddShow(parts);
                break;
            case "list-shows":
            case "4":
                HandleListShows(parts);
                break;
            case "book":
            case "5":
                HandleBook(parts);
                break;
            case "marathon":
            case "6":
                HandleMarathon(parts);
                break;
            default:
                Console.WriteLine($"\nUnknown command: '{cmd}'. Type 'menu' for help.");
                break;
        }
    }

    private static void HandleAddRoom(string[] parts)
    {
        if (parts.Length != 3)
        {
            Console.WriteLine("Usage: add-room \"<name>\" <capacity>");
            return;
        }

        var name = parts[1];
        if (!int.TryParse(parts[2], out var capacity) || capacity <= 0)
        {
            Console.WriteLine("Invalid capacity. Must be a positive number.");
            return;
        }

        if (rooms.ContainsKey(name))
        {
            Console.WriteLine($"Warning: Room '{name}' already exists. Updating...");
        }

        rooms[name] = new Room(name, capacity);
        Console.WriteLine($"Added room: {name} ({capacity} seats)");
    }

    private static void AddRoomInteractive()
    {
        Console.WriteLine("\n[Add Room]");
        Console.Write("Name: ");
        var name = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Capacity: ");
        var capStr = Console.ReadLine()?.Trim() ?? "";
        HandleAddRoom(new[] { "add-room", name, capStr });
    }

    private static void HandleAddMovie(string[] parts)
    {
        if (parts.Length != 3)
        {
            Console.WriteLine("Usage: add-movie \"<title>\" <duration>");
            return;
        }

        var title = parts[1];
        if (!TimeSpan.TryParse(parts[2], out var duration) || duration <= TimeSpan.Zero)
        {
            Console.WriteLine("Invalid duration. Use format HH:mm");
            return;
        }

        if (movies.ContainsKey(title))
        {
            Console.WriteLine($"Warning: Movie '{title}' already exists. Updating...");
        }

        movies[title] = new Movie(title, duration);
        Console.WriteLine($"Added movie: {title} ({duration.Hours}h {duration.Minutes}m)");
    }

    private static void AddMovieInteractive()
    {
        Console.WriteLine("\n[Add Movie]");
        Console.Write("Title: ");
        var title = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Duration (HH:mm): ");
        var durStr = Console.ReadLine()?.Trim() ?? "";
        HandleAddMovie(new[] { "add-movie", title, durStr });
    }

    private static void HandleAddShow(string[] parts)
    {
        if (parts.Length != 5)
        {
            Console.WriteLine("Usage: add-show \"<movie>\" \"<room>\" <datetime> <price>");
            return;
        }

        var movieTitle = parts[1];
        var roomName = parts[2];

        if (!movies.TryGetValue(movieTitle, out var movie))
        {
            Console.WriteLine($"Movie '{movieTitle}' not found. Add it first.");
            if (movies.Count > 0)
            {
                Console.WriteLine($"Available: {string.Join(", ", movies.Keys)}");
            }
            return;
        }

        if (!rooms.TryGetValue(roomName, out var room))
        {
            Console.WriteLine($"Room '{roomName}' not found. Add it first.");
            if (rooms.Count > 0)
            {
                Console.WriteLine($"Available: {string.Join(", ", rooms.Keys)}");
            }
            return;
        }

        if (!DateTime.TryParse(parts[3], out var start))
        {
            Console.WriteLine("Invalid datetime. Use format: yyyy-MM-ddTHH:mm");
            return;
        }

        if (!decimal.TryParse(parts[4], out var price) || price < 0)
        {
            Console.WriteLine("Invalid price. Must be a non-negative number.");
            return;
        }

        var show = new Show(movie, room, start, price);
        store.Add(show);
        Console.WriteLine("Show created:");
        Console.WriteLine($"  ID: {show.Id:N}");
        Console.WriteLine($"  Movie: {show.Movie.Title}");
        Console.WriteLine($"  Room: {show.Room.Name}");
        Console.WriteLine($"  Time: {show.Start:yyyy-MM-dd HH:mm} - {show.End:HH:mm}");
        Console.WriteLine($"  Price: ${show.Price:F2}");
        Console.WriteLine($"  Seats: {show.AvailableSeats}/{show.Room.Capacity}");
    }

    private static void AddShowInteractive()
    {
        Console.WriteLine("\n[Add Show]");
        Console.Write("Movie title: ");
        var movie = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Room name: ");
        var room = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Start time (yyyy-MM-ddTHH:mm): ");
        var time = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Price: ");
        var price = Console.ReadLine()?.Trim() ?? "";
        HandleAddShow(new[] { "add-show", movie, room, time, price });
    }

    private static void HandleListShows(string[] parts)
    {
        if (parts.Length != 2)
        {
            Console.WriteLine("Usage: list-shows <date>");
            return;
        }

        if (!DateOnly.TryParse(parts[1], out var day))
        {
            Console.WriteLine("Invalid date. Use format: yyyy-MM-dd");
            return;
        }

        var shows = store.All()
            .Where(s => DateOnly.FromDateTime(s.Start) == day)
            .OrderBy(s => s.Start)
            .ToList();

        if (shows.Count == 0)
        {
            Console.WriteLine($"\nNo shows on {day:yyyy-MM-dd}");
            return;
        }

        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"SHOWS FOR {day:yyyy-MM-dd} ({shows.Count})");
        Console.WriteLine($"{new string('=', 80)}\n");

        for (int i = 0; i < shows.Count; i++)
        {
            var s = shows[i];
            Console.WriteLine($"#{i + 1}");
            Console.WriteLine($"  ID: {s.Id:N}");
            Console.WriteLine($"  Movie: {s.Movie.Title}");
            Console.WriteLine($"  Room: {s.Room.Name}");
            Console.WriteLine($"  Time: {s.Start:yyyy-MM-dd HH:mm} - {s.End:HH:mm}");
            Console.WriteLine($"  Price: ${s.Price:F2}");
            Console.WriteLine($"  Seats: {s.AvailableSeats}/{s.Room.Capacity} available");
            if (i < shows.Count - 1) Console.WriteLine();
        }

        Console.WriteLine($"\n{new string('=', 80)}");
    }

    private static void ListShowsInteractive()
    {
        Console.WriteLine("\n[List Shows]");
        Console.Write("Date (yyyy-MM-dd): ");
        var date = Console.ReadLine()?.Trim() ?? "";
        HandleListShows(new[] { "list-shows", date });
    }

    private static void HandleBook(string[] parts)
    {
        if (parts.Length < 3)
        {
            Console.WriteLine("Usage: book <showId> <seat1> [seat2] ...");
            return;
        }

        if (!Guid.TryParse(parts[1], out var showId))
        {
            Console.WriteLine("Invalid show ID format.");
            return;
        }

        var show = store.Find(showId);
        if (show == null)
        {
            Console.WriteLine("Show not found.");
            return;
        }

        var seats = new List<int>();
        for (int i = 2; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out var seat))
            {
                Console.WriteLine($"Invalid seat number: {parts[i]}");
                return;
            }
            seats.Add(seat);
        }

        var invalid = seats.Where(s => s < 1 || s > show.Room.Capacity).ToList();
        if (invalid.Any())
        {
            Console.WriteLine($"Invalid seats: {string.Join(", ", invalid)}");
            Console.WriteLine($"Seats must be between 1 and {show.Room.Capacity}");
            return;
        }

        if (show.TryBook(seats.ToArray()))
        {
            Console.WriteLine($"Booked {seats.Count} seat(s): {string.Join(", ", seats)}");
            Console.WriteLine($"Show: {show.Movie.Title} at {show.Start:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Remaining: {show.AvailableSeats}/{show.Room.Capacity}");
        }
        else
        {
            Console.WriteLine("Booking failed. Some seats may be taken.");
            Console.WriteLine($"Remaining: {show.AvailableSeats}/{show.Room.Capacity}");
        }
    }

    private static void BookInteractive()
    {
        Console.WriteLine("\n[Book Seats]");
        Console.Write("Show ID: ");
        var id = Console.ReadLine()?.Trim() ?? "";
        Console.Write("Seat numbers (space-separated): ");
        var seatsStr = Console.ReadLine()?.Trim() ?? "";
        var seatParts = seatsStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parts = new List<string> { "book", id };
        parts.AddRange(seatParts);
        HandleBook(parts.ToArray());
    }

    private static void HandleMarathon(string[] parts)
    {
        if (parts.Length != 2)
        {
            Console.WriteLine("Usage: marathon <date>");
            return;
        }

        if (!DateOnly.TryParse(parts[1], out var day))
        {
            Console.WriteLine("Invalid date. Use format: yyyy-MM-dd");
            return;
        }

        var plan = planner.Plan(day, store.All());
        if (plan.Count == 0)
        {
            Console.WriteLine($"\nNo marathon plan for {day:yyyy-MM-dd}");
            return;
        }

        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"MARATHON PLAN - {day:yyyy-MM-dd}");
        Console.WriteLine($"{new string('=', 80)}");
        Console.WriteLine($"\nYou can watch {plan.Count} movie(s)!\n");

        var totalTime = TimeSpan.Zero;
        var totalCost = 0m;

        for (int i = 0; i < plan.Count; i++)
        {
            var s = plan[i];
            totalTime += s.Movie.Duration;
            totalCost += s.Price;

            Console.WriteLine($"Movie #{i + 1}");
            Console.WriteLine($"  ID: {s.Id:N}");
            Console.WriteLine($"  Title: {s.Movie.Title}");
            Console.WriteLine($"  Room: {s.Room.Name}");
            Console.WriteLine($"  Start: {s.Start:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"  End: {s.End:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"  Duration: {s.Movie.Duration.Hours}h {s.Movie.Duration.Minutes}m");
            Console.WriteLine($"  Price: ${s.Price:F2}");
            Console.WriteLine($"  Seats: {s.AvailableSeats}/{s.Room.Capacity}");

            if (i < plan.Count - 1)
            {
                var gap = plan[i + 1].Start - s.End;
                if (gap.TotalMinutes > 0)
                {
                    Console.WriteLine($"  Break: {gap.TotalMinutes} minutes");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine("SUMMARY");
        Console.WriteLine($"{new string('=', 80)}");
        Console.WriteLine($"  Movies: {plan.Count}");
        Console.WriteLine($"  Total Time: {totalTime.Hours}h {totalTime.Minutes}m");
        Console.WriteLine($"  Total Cost: ${totalCost:F2}");
        if (plan.Count > 0)
        {
            Console.WriteLine($"  Avg Price: ${totalCost / plan.Count:F2}");
        }
        Console.WriteLine($"\n{new string('=', 80)}");
    }

    private static void MarathonInteractive()
    {
        Console.WriteLine("\n[Marathon Plan]");
        Console.Write("Date (yyyy-MM-dd): ");
        var date = Console.ReadLine()?.Trim() ?? "";
        HandleMarathon(new[] { "marathon", date });
    }

    private static void ShowWelcome()
    {
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("CINEMA MANAGEMENT SYSTEM".PadLeft(45));
        Console.WriteLine(new string('=', 80));
        Console.WriteLine();
    }

    private static void ShowMenu()
    {
        Console.WriteLine("MENU:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine();
        Console.WriteLine("  [0] Exit");
        Console.WriteLine();
        Console.WriteLine("  Management:");
        Console.WriteLine("    [1] Add Room");
        Console.WriteLine("    [2] Add Movie");
        Console.WriteLine("    [3] Add Show");
        Console.WriteLine();
        Console.WriteLine("  View & Book:");
        Console.WriteLine("    [4] List Shows");
        Console.WriteLine("    [5] Book Seats");
        Console.WriteLine("    [6] Marathon Plan");
        Console.WriteLine();
        Console.WriteLine("  [menu/help] - Show this menu");
        Console.WriteLine();
        Console.WriteLine(new string('-', 80));
        Console.WriteLine();
        Console.WriteLine("Tip: Type a number (0-6) or the full command");
        Console.WriteLine();
    }

    private static string[] Split(string input)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current);
                    current = "";
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
        {
            result.Add(current);
        }

        return result.ToArray();
    }
}
