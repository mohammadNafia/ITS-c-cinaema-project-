using System;
using System.Collections.Generic;
using CinemaApp.Models;

namespace CinemaApp.Services;

public interface IShowStore
{
    IEnumerable<Show> All();
    void Add(Show show);
    Show? Find(Guid id);
}
