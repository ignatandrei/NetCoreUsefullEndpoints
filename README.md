# NetCoreUsefullEndpoints

[![GitHub last commit](https://img.shields.io/github/last-commit/ignatandrei/NetCoreUsefullEndpoints?label=updated)](https://github.com/ignatandrei/NetCoreUsefullEndpoints)
[![Nuget](https://img.shields.io/nuget/v/NetCoreUsefullEndpoints)](https://www.nuget.org/packages/NetCoreUsefullEndpoints)
[![NuGet Badge](https://buildstats.info/nuget/NetCoreUsefullEndpoints)](https://www.nuget.org/packages/NetCoreUsefullEndpoints/)

# What it does

Register endpoints for

1. See environment variables
2. See current user ( implies authorization )
3. See environment
3. Throw error ( with ILogger or without )
4. Current Date
5. Digraph of current endpoints
6. JSON of current endpoints
7. Configuration View ( GetDebugView )
8. Connection details ( remote ip address, local ip address...)
9. Shutdown( and forced) the app ( use await app.RunAsync(UsefullExtensions.UsefullExtensions.cts.Token);
10. See the start date of the application
11. See the list of Hosted services / start all / stop all
12. When the PC has started ( uptime )
13. Information about the process ( memory, threads, handles, uptime )
14. Information about adresses
15. Information about the RuntimeInformation ( OS, Framework)
16. Info about the user is in role or not

# Usage
For .NET 9 , add this reference to your csproj

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="9.2025.405.1013" />
  </ItemGroup>

```


For .NET 8 , add this reference to your csproj

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="8.2024.906.1703" />
  </ItemGroup>

```

Add this reference to your csproj in .NET 6

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="6.2022.1231.1100" />
  </ItemGroup>

```
or in .NET 7

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="7.2023.1216.1825" />
  </ItemGroup>

```

then use it in program.cs

```csharp
using UsefullExtensions;
//code
var app = builder.Build();
app.MapUsefullAll();
app.MapHostedServices(app.Services.GetServices<IHostedService>().ToArray());
//or for just some usefull
app.MapUsefullConfiguration();
```

For shutdown 418 please add
```csharp
builder.Services.AddSingleton<MiddlewareShutdown>();
var app = builder.Build();
//ASAP
app.UseMiddleware<MiddlewareShutdown>();
```

The list of API endpoints is
GET=>/api/usefull/date/start
GET=>/api/usefull/date/startUTC
GET=>/api/usefull/user/authorization
GET=>/api/usefull/user/noAuthorization
GET=>/api/usefull/environment
GET=>/api/usefull/errorWithILogger
GET=>/api/usefull/errorPure
GET=>/api/usefull/date/now
GET=>/api/usefull/date/nowUTC
GET=>/api/usefull/endpoints/graph
GET=>/api/usefull/endpoints/text
GET=>/api/usefull/configuration
GET=>/api/usefull/httpContext/Connection
POST=>/api/usefull/shutdown
POST=>/api/usefull/shutdownForced/{id}
GET=>api/usefull/user/isInRole/{role}
GET=>api/usefull/user/claims/simple
  
# Security

Each function has a default implementation with AllowAnonymous ( a part user ) and without put RequireCors ;
If you want a special case here, call the functions with
```csharp
app.MapUsefullAll("myCors", new string[] {"myAuthPolicy"});
//or
app.MapUsefullConfiguration();
```
For restarting , the last line should be
```csharp
await app.RunAsync(UsefullExtensions.UsefullExtensions.cts.Token);
```

# Enjoy!
