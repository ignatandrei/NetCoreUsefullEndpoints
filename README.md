# NetCoreUsefullEndpoints
[![CI build status](https://github.com/ignatandrei/NetCoreUsefullEndpoints/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/ignatandrei/NetCoreUsefullEndpoints/actions/workflows/dotnet.yml)
[![NuGet Package](https://img.shields.io/nuget/v/NetCoreUsefullEndpoints?logo=nuget)](https://www.nuget.org/packages/NetCoreUsefullEndpoints)
[![Project license](https://img.shields.io/github/license/ignatandrei/NetCoreUsefullEndpoints)](LICENSE)

# What it does

Register endpoints for

1. See environment variables
2. See current user
3. Throw error ( with ILogger or without )
4. Current Date


# Usage
Add this reference

```xml
<ItemGroup>
    <PackageReference Include="NetCoreUsefullEndpoints" Version="6.2022.722.712" />
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


GET=>/api/usefull/user

GET=>/api/usefull/environment

GET=>/api/usefull/errorWithILogger

GET=>/api/usefull/errorPure

GET=>/api/usefull/date

GET=>/api/usefull/graph/text


# Security

Each function has a default implementation with AllowAnonymous ( a part user ) and without put RequireCors ;
If you want a special case here, call the functions with
```csharp
app.MapUsefullAll("myCors", new string[] {"myAuthPolicy"});
//or
app.MapUsefullConfiguration();
```


# Enjoy!
