IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);


 IResourceBuilder<PostgresServerResource> db = builder.AddPostgres("facturxmanager-db")
    .WithDataVolume()
    .WithPgAdmin();


builder.AddProject<Projects.FacturXManager_Api>("FacturXManager-api")
    .WithReference(db)
    .WaitFor(db);


await builder.Build().RunAsync();
