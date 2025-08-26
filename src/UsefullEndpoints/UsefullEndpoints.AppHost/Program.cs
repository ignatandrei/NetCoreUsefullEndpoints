using WebAPIDocsExtensionAspire;
var builder = DistributedApplication.CreateBuilder(args);

var webAPI = builder.AddProject<Projects.TestUsefullEndpoints>("testusefullendpoints");
webAPI.AddSDKGeneration_openapitools(jsonPath: "swagger/v1/swagger.json");
builder.Build().Run();
