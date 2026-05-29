
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "ASP.NET Core controllers and middleware run without a SynchronizationContext.")]

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods",
    Justification = "[ApiController] validates request models before controller actions are executed.")]

[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates",
    Justification = "Exception handling middleware is not a hot path.")]
