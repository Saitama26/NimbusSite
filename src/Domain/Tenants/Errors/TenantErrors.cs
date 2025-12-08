using SharedKernel;

namespace Domain.Tenants.Errors;

public static class TenantErrors
{
    public static Error NotFound(Guid tenantId) =>
        new Error("Tenant.NotFound",
            $"The tenant with the id - {tenantId} was not found",
            ErrorType.NotFound);

    public static Error AlreadyExists(string name) =>
        new Error("Tenant.AlreadyExists",
            $"The tenant with the name '{name}' already exists",
            ErrorType.Conflict);

    public static Error InvalidConnectionString() =>
        new Error("Tenant.InvalidConnectionString",
            "The connection string format is invalid.",
            ErrorType.Validation);
}

