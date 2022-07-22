# NetCoreUsefullEndpoints

Usefull Endpoints for .NET Core

# What it does

Register endpoints for

1. See environment variables
2. See current user
3. Throw error ( with ILogger or without )


# Usage

```csharp
using UsefullExtensions;
//code
var app = builder.Build();
app.MapAllUsefull();

```

The list of API endpoints is

GET => /api​/usefull​/user

GET => ​/api​/usefull​/environment

GET => ​/api​/usefull​/errorWithILogger

GET => ​/api​/usefull​/errorPure

GET => ​/api​/usefull​/graph​/text

# Enjoy!