using System;
using System.Collections.Generic;
using System.Linq;
using CinemaApp.Models;

namespace CinemaApp.Services;

public sealed class GreedyMarathonPlanner : IMarathonPlanner
{
    public List<Show> Plan(DateOnly day, IEnumerable<Show> shows)
    {
        if (shows == null)
            throw new ArgumentNullException(nameof(shows));

        var dayShows = shows
            .Where(s => DateOnly.FromDateTime(s.Start) == day)
            .OrderBy(s => s.End)
            .ThenBy(s => s.Start)
            .ToList();

        var plan = new List<Show>();
        var currentEndTime = DateTime.MinValue;

        foreach (var show in dayShows)
        {
            if (show.Start >= currentEndTime)
            {
                plan.Add(show);
                currentEndTime = show.End;
            }
        }

        return plan;
    }
}
