# NetCoreUsefullEndpoints
[![CI build status](https://github.com/ignatandrei/NetCoreUsefullEndpoints/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/ignatandrei/NetCoreUsefullEndpoints/actions/workflows/dotnet.yml)
[![NuGet Package](https://img.shields.io/nuget/v/NetCoreUsefullEndpoints?logo=nuget)](https://www.nuget.org/packages/NetCoreUsefullEndpoints)
[![Project license](https://img.shields.io/github/license/ignatandrei/NetCoreUsefullEndpoints)](LICENSE)

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
9. Restart the app ( use await app.RunAsync(UsefullExtensions.UsefullExtensions.cts.Token);
# Usage
Add this reference to your csproj in .NET 6

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="7.2022.1231.1100" />
  </ItemGroup>

```
or in .NET 7

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="7.2022.1231.1100" />
  </ItemGroup>

```

then use it in program.cs

```csharp
using UsefullExtensions;
//code
var app = builder.Build();
app.MapUsefullAll();
//or for just some usefull
app.MapUsefullConfiguration();
```

The list of API endpoints is



GET=>/api/usefull/user/authorization

GET=>/api/usefull/user/noAuthorization

GET=>/api/usefull/environment

GET=>/api/usefull/errorWithILogger

GET=>/api/usefull/errorPure

GET=>/api/usefull/date

GET=>/api/usefull/endpoints/graph

GET=>/api/usefull/endpoints/text

GET=>/api/usefull/configuration

GET=>/api/usefull/httpContext/Connection

POST=>/api/usefull/shutdown


# Security

Each function has a default implementation with AllowAnonymous ( a part user ) and without put RequireCors ;
If you want a special case here, call the functions with
```csharp
app.MapUsefullAll("myCors", new string[] {"myAuthPolicy"});
//or
app.MapUsefullConfiguration();
```


# Enjoy!
