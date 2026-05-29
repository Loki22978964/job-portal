
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods",
    Justification = "EF Core-generated migration methods; MigrationBuilder is guaranteed non-null by the runtime.",
    Scope = "namespace", Target = "~N:DAL.Migrations")]

[assembly: SuppressMessage("Performance", "CA1861:Prefer static readonly fields over constant array arguments",
    Justification = "EF Core-generated migration; inline arrays are scaffolded automatically.",
    Scope = "namespace", Target = "~N:DAL.Migrations")]
