using System;
using System.Diagnostics;

namespace Wasmtime
{
    /// <summary>
    /// Represents a memory exported from a WebAssembly module.
    /// </summary>
    public class MemoryExport : Export
    {
        internal MemoryExport(IntPtr exportType, IntPtr externType) : base(exportType)
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