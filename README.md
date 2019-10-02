# Wasmtime.NET

A .NET API for [Wasmtime](https://github.com/CraneStation/wasmtime).

Wasmtime is a standalone runtime for [WebAssembly](https://webassembly.org/), using the Cranelift JIT compiler.

Wasmtime.NET allows .NET code to instantiate WebAssembly modules and to interact with them in-process.

# Getting Started

## Prerequisites

### .NET Core 3.0

Install a [.NET Core 3.0+ SDK](https://dotnet.microsoft.com/download) for your operating system.

## Introduction to Wasmtime.NET

See the [introduction to Wasmtime.NET](https://peterhuene.github.io/wasmtime.net/articles/intro.html) for a complete walkthrough of how to use Wasmtime.NET.

# Wasmtime.NET API documentation

See the [Wasmtime.NET API documentation](https://peterhuene.github.io/wasmtime.net/api/index.html) for documentation on using the Wasmtime.NET types.

# Running the "Hello World" Example

The "hello world" example demonstrates a simple C# function being called from WebAssembly.

To run the "hello world" example, follow these instructions:

1. `cd examples/hello`
2. `dotnet run`

You should see a `Hello from C#, WebAssembly!` message printed.

# Building Wasmtime.NET

To build Wasmtime.NET, follow these instructions:

1. `cd src`.
2. `dotnet build`.

This should produce a `Wasmtime.dll` assembly in the `bin/Debug/netstandard2.1` directory.

# Creating the Wasmtime.NET NuGet package

To create the Wasmtime.NET NuGet package, follow these instructions:

1. `cd src`.
2. `dotnet pack`.

This should produce a `Wasmtime.<version>.nupkg` assembly in the `bin/Debug` directory.

# Running the tests

To run the Wasmtime.NET unit tests, follow these instructions:

1. `cd tests`.
2. `dotnet test`.

# Wasmtime API

Snapshots of the Wasmtime API libraries exist in this repository in the `wasmtime` directory.  These files are used for packaging the `Wasmtime` NuGet package.

If needed, the Wasmtime API libraries may be updated from the latest development snapshots for your platform:

| Operating System | Link                                                                                                              |
|------------------|-------------------------------------------------------------------------------------------------------------------|
| Linux (x86_64)   | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-linux-c-api.tar.xz) |
| macOS (x86_64)   | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-macos-c-api.tar.xz) |
| Windows (x64)    | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-windows-c-api.zip)  |

# Implementation Status

## Status

| Feature                               | Status |
|---------------------------------------|--------|
| Wasmtime engine class                 | ✅     |
| Wasmtime store class                  | ✅     |
| Wasmtime module class                 | ✅     |
| Wasmtime instance class               | 🔄     |
| Module function imports               | ✅     |
| Module global imports                 | ✅     |
| Module table imports                  | ✅     |
| Module memory imports                 | ✅     |
| Module function exports               | ✅     |
| Module global exports                 | ✅     |
| Module table exports                  | ✅     |
| Module memory exports                 | ✅     |
| Extern instance functions             | ✅     |
| Extern instance globals               | ✅️     |
| Extern instance tables                | ⬜️     |
| Extern instance memories              | ✅️     |
| Host function import binding          | ✅     |
| Host global import binding            | ✅ ️️    |
| Host table import binding             | ⬜️ ️️    |
| Host memory import binding            | ✅️ ️️    |
| `i32` parameters and return values    | ✅     |
| `i64` parameters and return values    | ✅     |
| `f32` parameters and return values    | ✅     |
| `f64` parameters and return values    | ✅     |
| `AnyRef` parameters and return values | ⬜️     |
| Tuple return types for host functions | ✅     |
| Trap messages                         | ✅     |
| Trap frames                           | ⬜️     |
| Create a NuGet package                | ✅     |

## Legend

| Status | Icon |
|-----------------|--------|
| Not implemented | ⬜️     |
| In progress     | 🔄     |
| Completed       | ✅     |