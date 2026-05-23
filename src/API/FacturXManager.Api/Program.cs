using FacturXManager.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException(
        "La chaîne de connexion 'Database' est obligatoire (section ConnectionStrings).");

await builder.Services.AddFacturXManagerAsync(connectionString, builder.Configuration);

WebApplication app = builder.Build();

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
