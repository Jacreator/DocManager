using DocManager.Application.Abstractions;

namespace DocManager.Tests.Support;

public sealed class TestCurrentCompany(int companyId) : ICurrentCompany
{
    public int CompanyId { get; } = companyId;
}
