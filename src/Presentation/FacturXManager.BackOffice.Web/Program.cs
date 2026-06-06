using FacturXManager.BackOffice.Web.Components;
using FacturXManager.Client;
using Radzen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Radzen UI services (notifications, dialogs, tooltips, context menu).
builder.Services.AddRadzenComponents();

// MediatR orchestration for front use cases (logique de vue uniquement).
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Refit client typé vers le backend FacturX Manager.
string backendBaseUrl =
    builder.Configuration["services:facturxmanager-api:https:0"] 
        ?? builder.Configuration["services:facturxmanager-api:http:0"] 
        ?? builder.Configuration.GetValue<string>("BackendApi:BaseUrl")
        ?? throw new InvalidOperationException("BackendApi:BaseUrl n'est pas configuré.");

builder.Services.AddFacturXManagerClient(backendBaseUrl);

builder.Configuration.AddEnvironmentVariables();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
