using DocManager.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace DocManager.Web.Services;

/// <summary>
/// Resolves the company from the signed-in user's "company_id" claim, falling back to configuration.
/// </summary>
public sealed class HttpCurrentCompany(IHttpContextAccessor httpContextAccessor, IOptions<CompanyOptions> options)
    : ICurrentCompany
{
    public const string CompanyIdClaimType = "company_id";

    // TODO: Once real authentication (e.g. Microsoft Entra ID) is added, require the claim
    // and remove the configuration fallback so every request is tied to an authenticated company.
    public int CompanyId =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(CompanyIdClaimType)?.Value, out var companyId)
            ? companyId
            : options.Value.DefaultCompanyId;
}
