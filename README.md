# PgRoutiner

**.NET Core tool for easy scaffolding of your PostgreSQL server.**

Make your .NET Core project to do a **static type checking** - on your PostgreSQL.

This tool will generate **all the necessary source code files** needed to make a simple execution of your **PostgreSQL routines (functions or procedures):**

- Simple execution - or data retrieval - in C#, sync, or async.

- All the necessary data-access code as **connection object extension.** 

- All related **model classes** (or Data Transfer Object) for data retrieval operations (function returning recordset or physical table returned from a function).

You can use this tool to enforce **static type checking** over PostgreSQL programable routines (functions or procedures) - in your .NET Core project.

Simply add the code generation command with this tool to your pre-build event.

Or, you can simply just generate the code you need with a simple command. 

It will take care of things like:

- PostgreSQL function overload for multiple versions.

- PostgreSQL array types for complex input and output.

- Serialization of results into class models [faster](https://github.com/vbilopav/NoOrm.Net/blob/master/PERFOMANCE-TESTS.md) than any standard mapping mechanism (like Dapper or EF) and without any caching. Thanks to the innovative mapping mechanism by position implemented in [Norm.net](https://github.com/vbilopav/NoOrm.Net) data access.

## Installation

.NET global tool install:

```
dotnet tool install --global PgRoutiner
```

## Running

`PgRoutiner` tool is implemented as a command-line tool. 

It is enough to just type **`PgRoutiner`** and it will look for .NET Core project file (`.csproj`) in the current directory - and start source file generation by using first available connection string in your configuration.

Or... you may supply additional configuration settings trough either:

1) Custom **JSON configuration settings** section `PgRoutiner`. It is your standard `appsettings.json` or `appsettings.Development.json` from your project. For example, to configure the connection that will be used:

```json
{
  "PgRoutiner": {
    "Connection": "MyConnection",
  }
}
```

2) Standard command-line interface, by supplying command-line arguments. Example from above, to configure the connection that will be used:

```
PgRoutiner Connection=MyConnection
```
The command-line settings if supplied - **will always override JSON configuration settings.**

## Configuration

| Name | Description | Default |
| ---- | ----------- | ------- |
| **Connection** | Connection string name from your configuration connection string to be used. | First available connection string. |
| **Project** | Relative path to project `.csproj` file. | First available `.csproj` file from the current dir. |
| **OutputDir** | Relative path where generated source files will be saved. | Current dir. |
| **ModelDir** | Relative path where model classes source files will be saved. | Default value saves model classes in the same file as a related data-access code. |
| **Schema** | PostgreSQL schema name used to search for routines.  | public |
| **Overwrite** | Should existing generated source file be overwritten (true) or skipped if they exist (false) | true |
| **Namespace** |  Root namespace for generated source files. | Project root namespace. |
| **NotSimilarTo** | `NOT SIMILAR TO` SQL regular expression used to search routine names. | Default skips this matching. |
| **SimilarTo** | `SIMILAR TO` SQL regular expression used to search routine names. | Default skips this matching. |
| **SourceHeader** | Insert the following content to the start of each generated source code file. | `// <auto-generated at timestamp />` |
| **SyncMethod** | Generate a `sync` method, true or false. |  True. |
| **AsyncMethod** | Generate a `async` method, true or false. | True. |
| **Mapping** * | Key-values to override default type mapping. Key is PostgreSQL UDT type name and value is the corresponding C# type name. | See default mapping [here](/PgRoutiner/Settings.cs#L24)  |

* Key-values are JSON object in JSON configuration. For command-line, use following format: 
`PgRoutiner Mapping:Key=Value`

## Simple example

- PostgreSQL Function that returns `bigint` number:

```sql
create function calculate_some_number(_input_param bigint) returns bigint as
$$
begin
    -- calculate the number
    return _result;
end
$$
language plpgsql;
```

Running `PgRoutiner` tool will generate following source code:

```csharp
// <auto-generated at 2020-06-12T18:53:28.2799640+02:00 />
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Norm.Extensions;
using Npgsql;

namespace PgRoutiner.Test
{
    public static class PgRoutineCalculateSomeNumber
    {
        public const string Name = "calculate_some_number";

        /// <summary>
        /// sql function "calculate_some_number"
        /// description for calculate_some_number
        /// </summary>
        public static long CalculateSomeNumber(this NpgsqlConnection connection, long inputParam)
        {
            return connection
                .Single<long>(Name, ("_input_param", inputParam));
        }

        /// <summary>
        /// sql function "calculate_some_number"
        /// description for calculate_some_number
        /// </summary>
        public static async ValueTask<long> CalculateSomeNumberAsync(this NpgsqlConnection connection, long inputParam)
        {
            return await connection
                .SingleAsync<long>(Name, ("_input_param", inputParam));
        }
    }
}
```

For more examples go to this page...


