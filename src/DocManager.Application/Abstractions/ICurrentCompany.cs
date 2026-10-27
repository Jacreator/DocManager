namespace DocManager.Application.Abstractions;

/// <summary>Provides the company (tenant) the current request operates on.</summary>
public interface ICurrentCompany
{
    int CompanyId { get; }
}
