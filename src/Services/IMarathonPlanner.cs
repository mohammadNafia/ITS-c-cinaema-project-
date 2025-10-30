using System;
using System.Collections.Generic;
using CinemaApp.Models;

namespace CinemaApp.Services;

public interface IMarathonPlanner
{
    List<Show> Plan(DateOnly day, IEnumerable<Show> shows);
}
