using DocManager.Application;
using DocManager.Application.Abstractions;
using DocManager.Application.Options;
using DocManager.Infrastructure;
using DocManager.Web.ErrorHandling;
using DocManager.Web.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOptions<DocumentOptions>()
    .Bind(builder.Configuration.GetSection(DocumentOptions.SectionName))
    .Validate(o => o.MaxFileSizeBytes > 0 && o.PageSize > 0 && o.AllowedExtensions.Length > 0,
        $"The '{DocumentOptions.SectionName}' configuration section is invalid.")
    .ValidateOnStart();
builder.Services.Configure<CompanyOptions>(builder.Configuration.GetSection(CompanyOptions.SectionName));
builder.Services.AddScoped<ICurrentCompany, HttpCurrentCompany>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.Use((context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    return next(context);
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Documents}/{action=Index}/{id?}");

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await app.Services.MigrateDatabaseAsync();
}

await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program
{
}
