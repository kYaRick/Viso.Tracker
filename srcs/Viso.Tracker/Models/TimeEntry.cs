namespace Viso.Tracker.Models;

/// <summary>
/// Represents a single time tracking entry.
/// </summary>
public class TimeEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date of the entry.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Project { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of hours worked.
    /// </summary>
    public decimal Hours { get; set; }

    /// <summary>
    /// Gets or sets the work description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
