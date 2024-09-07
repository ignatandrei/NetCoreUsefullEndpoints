var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TestUsefullEndpoints>("testusefullendpoints");

builder.Build().Run();
