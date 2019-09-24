using System;
using System.Runtime.InteropServices;

namespace Wasmtime
{
    /// <summary>
    /// The base class for import types.
    /// </summary>
    public abstract class Import
    {
        internal Import(IntPtr importType)
        {
            unsafe
            {
                var module = Interop.wasm_importtype_module(importType);
                Module = Marshal.PtrToStringUTF8((IntPtr)module->data, (int)module->size);

                var name = Interop.wasm_importtype_name(importType);
                Name = Marshal.PtrToStringUTF8((IntPtr)name->data, (int)name->size);
            }
        }

        /// <summary>
        /// The module name of the import.
        /// </summary>
        public string Module { get; private set; }

        /// <summary>
        /// The name of the import.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Module}{(string.IsNullOrEmpty(Module) ? "" : ".")}{Name}";
        }
    }
}