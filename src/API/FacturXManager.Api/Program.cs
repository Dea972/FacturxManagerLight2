using FacturXManager.Sdk;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

await builder.Services.AddFacturXManagerAsync( builder.Configuration);

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddUserSecrets<Program>();

WebApplication app = builder.Build();
if (!app.Environment.IsProduction())
{
   app.ApplyMigrations();
}
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapFacturXManagerEndpoints();

await app.RunAsync();

// Marqueur exposé pour WebApplicationFactory<Program> dans les tests d'intégration.
public partial class Program;
