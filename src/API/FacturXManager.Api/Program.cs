using System.Text.Json.Serialization;
using FacturXManager.Sdk;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.AddServiceDefaults();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "FacturX Manager API";
        document.Info.Version = "v1";
        document.Info.Description =
            "API de génération et transmission de factures électroniques Factur-X " +
            "(PDF/A-3b + XML CII embarqué) vers Chorus Pro via PISTE (AIFE). " +
            "Réforme française de facturation électronique — septembre 2026.";

        return Task.CompletedTask;
    }));

await builder.Services.AddFacturXManagerAsync(builder.Configuration);

builder.Configuration.AddEnvironmentVariables();

builder.Configuration.AddUserSecrets<Program>();
WebApplication app = builder.Build();


app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FacturX Manager API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "FacturX Manager API";
    });
}

app.UseHttpsRedirection();

app.MapFacturXManagerEndpoints();

await app.RunAsync();

// Marqueur exposé pour WebApplicationFactory<Program> dans les tests d'intégration.
public partial class Program;
