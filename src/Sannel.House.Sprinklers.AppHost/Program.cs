var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Sannel_House_Sprinklers>("sprinklers-api");

builder.AddProject<Projects.Sannel_House_Sprinklers_Web>("sprinklers-web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
