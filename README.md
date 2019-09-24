# Wasmtime.NET

A .NET API for [Wasmtime](https://github.com/CraneStation/wasmtime).

Wasmtime is a standalone runtime for [WebAssembly](https://webassembly.org/) (WASM), using the Cranelift JIT compiler.

Wasmtime.NET allows .NET code to instantiate WASM modules and to interact with them in-process.

# Getting Started

## Building Wasmtime

TODO: add this section

## Building Wasmtime.NET

TODO: add this section

# Running the "Hello World" Example

TODO: add this section

# Implementation Status

#### Status

| Feature                               | Status |
|---------------------------------------|--------|
| Wasmtime engine class                 | ✅     |
| Wasmtime store class                  | ✅     |
| Wasmtime module class                 | ✅     |
| Wasmtime instance class               | 🔄     |
| Module function imports               | ✅     |
| Module global imports                 | ✅     |
| Module table imports                  | 🔄     |
| Module memory imports                 | 🔄     |
| Module function exports               | ✅     |
| Module global exports                 | ✅     |
| Module table exports                  | 🔄     |
| Module memory exports                 | 🔄     |
| Extern instance functions             | ✅     |
| Extern instance globals               | ⬜️     |
| Extern instance tables                | ⬜️     |
| Extern instance memories              | ⬜️     |
| Host function import binding          | ✅     |
| Host global import binding            | ⬜️ ️️    |
| Host table import binding             | ⬜️ ️️    |
| Host memory import binding            | ⬜️ ️️    |
| `i32` parameters and return values    | ✅     |
| `i64` parameters and return values    | ✅     |
| `f32` parameters and return values    | ✅     |
| `f64` parameters and return values    | ✅     |
| `AnyRef` parameters and return values | ⬜️     |
| Tuple return types for host functions | ✅     |
| Trap messages                         | ✅     |
| Trap frames                           | ⬜️     |

#### Legend

| Status | Icon |
|-----------------|--------|
| Not implemented | ⬜️     |
| In progress     | 🔄     |
| Completed       | ✅     |