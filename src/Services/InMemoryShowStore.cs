using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Models;

namespace CinemaApp.Services;

public sealed class InMemoryShowStore : IShowStore
{
    private readonly List<Show> _shows = new();

    public IEnumerable<Show> All() => _shows;

    public void Add(Show show)
    {
        if (show == null)
            throw new ArgumentNullException(nameof(show));
        _shows.Add(show);
    }

    public Show? Find(Guid id) => _shows.FirstOrDefault(show => show.Id == id);
}
