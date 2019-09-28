using System;

namespace Wasmtime
{
    /// <summary>
    /// Represents the Wasmtime engine.
    /// </summary>
    public class Engine : IDisposable
    {
        /// <summary>
        /// Constructs a new <see href="Engine" />.
        /// </summary>
        public Engine()
        {
            Handle = Interop.wasm_engine_new();

            if (Handle.IsInvalid)
            {
                throw new WasmtimeException("Failed to create Wasmtime engine.");
            }
        }

        /// <summary>
        /// Creates a new Wasmtime <see href="Store" />.
        /// </summary>
        /// <returns>Returns the new <see href="Store" />.</returns>
        public Store CreateStore()
        {
            return new Store(this);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Handle.IsInvalid)
            {
                Handle.Dispose();
                Handle.SetHandleAsInvalid();
            }
        }

        internal Interop.EngineHandle Handle { get; private set; }
    }
}
