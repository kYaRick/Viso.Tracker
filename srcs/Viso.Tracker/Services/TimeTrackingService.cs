namespace Viso.Tracker.Services;

using Viso.Tracker.Models;

/// <summary>
/// Service for managing time tracking entries in memory.
/// </summary>
public class TimeTrackingService
{
    private readonly List<TimeEntry> _entries = [];

    /// <summary>
    /// Gets the list of all time tracking entries.
    /// </summary>
    public IReadOnlyList<TimeEntry> Entries => _entries.AsReadOnly();

    /// <summary>
    /// Gets the predefined project options.
    /// </summary>
    public static IReadOnlyList<string> ProjectOptions => new[]
    {
        "Viso Internal",
        "Client A",
        "Client B",
        "Personal Development",
        "Research"
    };

    /// <summary>
    /// Adds a new time entry to the collection.
    /// </summary>
    public void AddEntry(TimeEntry entry)
    {
        if (entry is null)
            throw new ArgumentNullException(nameof(entry));

        _entries.Add(entry);
    }

    /// <summary>
    /// Gets all entries grouped by date, in descending order.
    /// </summary>
    public IEnumerable<IGrouping<DateOnly, TimeEntry>> GetEntriesGroupedByDate()
    {
        return _entries
            .GroupBy(e => e.Date)
            .OrderByDescending(g => g.Key);
    }

    /// <summary>
    /// Calculates the total hours for a specific date.
    /// </summary>
    public decimal GetDailyTotal(DateOnly date)
    {
        return _entries
            .Where(e => e.Date == date)
            .Sum(e => e.Hours);
    }

    /// <summary>
    /// Calculates the grand total hours across all entries.
    /// </summary>
    public decimal GetGrandTotal()
    {
        return _entries.Sum(e => e.Hours);
    }
}
