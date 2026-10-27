using DocManager.Application.Documents;
using DocManager.Application.Notes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DocManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UploadDocumentRequestValidator>(ServiceLifetime.Scoped);
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<INoteService, NoteService>();
        return services;
    }
}
