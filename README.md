# Wasmtime.NET

A .NET API for [Wasmtime](https://github.com/CraneStation/wasmtime).

Wasmtime is a standalone runtime for [WebAssembly](https://webassembly.org/), using the Cranelift JIT compiler.

Wasmtime.NET allows .NET code to instantiate WebAssembly modules and to interact with them in-process.

# Getting Started

## Prerequisites

### .NET Core 3.0

Install a [.NET Core 3.0+ SDK](https://dotnet.microsoft.com/download) for your operating system.

### Rust

If you intend to build your own Wasmtime API library, a Rust compiler will be required.

Install [rustup](https://rustup.rs/) to get started with a Rust toolchain.

### Wasmtime API

If you *do not* intend to build your own Wasmtime API library, you may download the latest development snapshot for your operating system:

| Operating System | Link                                                                                                              |
|------------------|-------------------------------------------------------------------------------------------------------------------|
| Linux (x86_64)   | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-linux-c-api.tar.xz) |
| macOS (x86_64)   | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-macos-c-api.tar.xz) |
| Windows (x64)    | [Download](https://github.com/CraneStation/wasmtime/releases/download/dev/wasmtime-dev-x86_64-windows-c-api.zip)  |

For Linux, copy `libwasmtime_api.so` to the directory containing your .NET program.

For macOS, copy `libwasmtime_api.dylib` to the directory containing your .NET program.

For Windows, copy `wasmtime_api.dll` to the directory containing your .NET program.

_NOTE: in the future there will be a "Wasmtime" NuGet package will come with the Wasmtime API library for all supported platforms._

### Building Wasmtime

If you intend to build your own Wasmtime API library, follow these instructions:

1. Clone or fork the [Wasmtime repository](https://github.com/cranestation/wasmtime). _Note that the repository uses Git submodules, so use the `--recurse-submodules` option when cloning._
2. cd `wasmtime`
3. `cargo build --release --features wasmtime-api/wasm-c-api --package wasmtime-api`

The Wasmtime API library should now be present in `<repo-root>/target/release`.

For Linux, the library will be named `libwasmtime_api.so`.

For MacOS, the library will be named `libwasmtime_api.dylib`.

For Windows, the library will be named `wasmtime_api.dll`.

Copy the Wasmtime API library to your .NET project output directory (e.g. `bin/Release/netcoreapp3.0/`).

## Running the "Hello World" Example

The "hello world" example demonstrates a simple C# function being called from WebAssembly.

To run the "hello world" example, follow these instructions:

1. `cd examples/hello`
2. `dotnet build`
3. Copy the Wasmtime API library to `bin/Debug/netcoreapp3.0`.
4. Run `bin/Debug/netcoreapp3.0/hello` (Linux/macOS) or `bin/Debug/netcoreapp3.0/hello.exe` (Windows).

You should see a `Hello from C#, WebAssembly!` message printed.

# Building Wasmtime.NET

To build Wasmtime.NET, follow these instructions:

1. `cd src`.
2. `dotnet build`.

This should produce a `Wasmtime.dll` assembly in `bin/Debug/netstandard2.1`.

# Running the tests

To run the Wasmtime.NET unit tests, follow these instructions:

1. `cd tests`.
2. `dotnet test`.

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

## Legend

| Status | Icon |
|-----------------|--------|
| Not implemented | ⬜️     |
| In progress     | 🔄     |
| Completed       | ✅     |