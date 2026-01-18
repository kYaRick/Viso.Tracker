using System.Threading.Tasks;

namespace Viso.Tracker;

public sealed record ThemeState(bool IsDarkMode, Func<Task> ToggleThemeAsync);
