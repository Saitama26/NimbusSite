using Application.Abstractions.Data;

namespace Infrastructure.Data;

internal sealed class TenantContext : ITenantContext
{
    private Guid? _tenantId;

    public Guid? TenantId => _tenantId;

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }

    public void ClearTenantId()
    {
        _tenantId = null;
    }
}

