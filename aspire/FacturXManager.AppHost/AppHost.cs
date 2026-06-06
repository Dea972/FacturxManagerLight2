IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);



IResourceBuilder<ProjectResource> api =  builder.AddProject<Projects.FacturXManager_Api>("facturxmanager-api");


builder.AddProject<Projects.FacturXManager_BackOffice_Web>("facturxmanager-backoffice-web")
    .WithReference(api)
    .WaitFor(api);


await builder.Build().RunAsync();
