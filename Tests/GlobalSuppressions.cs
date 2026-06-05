
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores",
    Justification = "Test method names follow the xUnit MethodName_Scenario_ExpectedResult convention.")]

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "xUnit runs tests without a SynchronizationContext.")]
