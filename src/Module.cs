using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wasmtime
{
    /// <summary>
    /// Represents a WASM module.
    /// </summary>
    public class Module : IDisposable
    {
        internal Module(Store store, string name, byte[] bytes)
        {
            if (store.Handle.IsInvalid)
            {
                throw new ArgumentNullException(nameof(store));
            }

            var bytesHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                unsafe
                {
                    Interop.wasm_byte_vec_t vec;
                    vec.size = (UIntPtr)bytes.Length;
                    vec.data = (byte*)bytesHandle.AddrOfPinnedObject();

                    Handle = Interop.wasm_module_new(store.Handle, ref vec);
                }

                if (Handle.IsInvalid)
                {
                    throw new WasmtimeException($"WASM module {name} is not valid.");
                }
            }
            finally
            {
                bytesHandle.Free();
            }

            Store = store;
            Name = name;
            Imports = new Imports(this);
            Exports = new Exports(this);
        }

        /// <summary>
        /// Instantiates a WASM module.
        /// </summary>
        /// <typeparam name="T">The host type defining the imports to the module.</typeparam>
        /// <returns>Returns a new <see href="Instance" />.</returns>
        public Instance Instantiate<T>() where T : Host, new()
        {
            var host = new T();
            host.Instance = new Instance(this, host);
            return host.Instance;
        }

        /// <summary>
        /// The <see href="Store"/> associated with the module.
        /// </summary>
        public Store Store { get; private set; }

        /// <summary>
        /// The name of the module.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The imports of the module.
        /// </summary>
        public Imports Imports { get; private set; }

        /// <summary>
        /// The exports of the module.
        /// </summary>
        /// <value></value>
        public Exports Exports { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Handle.IsInvalid)
            {
                Handle.Dispose();
                Handle.SetHandleAsInvalid();
            }
        }

        internal Interop.WasmtimeModuleHandle Handle { get; private set; }
    }
}
