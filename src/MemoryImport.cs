using System;
using System.Diagnostics;

namespace Wasmtime
{
    /// <summary>
    /// Represents a memory imported to a WASM module.
    /// </summary>
    public class MemoryImport : Import
    {
        internal MemoryImport(IntPtr importType, IntPtr externType) : base(importType)
        {
            Debug.Assert(Interop.wasm_externtype_kind(externType) == Interop.wasm_externkind_t.WASM_EXTERN_MEMORY);

            var memoryType = Interop.wasm_externtype_as_memorytype_const(externType);
        }

        /// <summary>
        /// The minimum memory size (in WebAssembly page units).
        /// </summary>
        public int Minimum { get; private set; }

        /// <summary>
        /// The maximum memory size (in WebAssembly page units).
        /// </summary>
        public int Maximum { get; private set; }
    }
}