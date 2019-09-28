using System;
using System.Runtime.InteropServices;

namespace Wasmtime
{
    /// <summary>
    /// Represents a WebAssembly global value.
    /// </summary>
    public class Global<T>
    {
        /// <summary>
        /// Creates a new <see href="Global" /> with the given initial value.
        /// </summary>
        /// <param name="initialValue">The initial value of the global.</param>
        /// <param name="isMutable">True if the global is mutable or false if it is not.static</param>
        public Global(T initialValue, bool isMutable = false)
        {
            _initialValue = initialValue;
            IsMutable = isMutable;
            Kind = Interop.ToValueKind(typeof(T));
        }

        /// <summary>
        /// The value of the global.
        /// </summary>
        public T Value
        {
            get
            {
                if (Handle == null)
                {
                    throw new InvalidOperationException("The global cannot be used before it is instantiated.");
                }

                unsafe
                {
                    var v = stackalloc Interop.wasm_val_t[1];

                    Interop.wasm_global_get(Handle.DangerousGetHandle(), v);

                    // TODO: figure out a way that doesn't box the value
                    return (T)Interop.ToObject(v);
                }
            }
            set
            {
                if (Handle == null)
                {
                    throw new InvalidOperationException("The global cannot be used before it is instantiated.");
                }

                if (!IsMutable)
                {
                    throw new InvalidOperationException("The value of the global cannot be modified.");
                }

                // TODO: figure out a way that doesn't box the value
                var v = Interop.ToValue(value, Kind);

                unsafe
                {
                    Interop.wasm_global_set(Handle.DangerousGetHandle(), &v);
                }
            }
        }

        internal void InitializeHandle(Interop.StoreHandle store)
        {
            if (Handle != null)
            {
                throw new InvalidOperationException("Handle has already been created.");
            }

            unsafe
            {
                var v = Interop.ToValue(_initialValue, Kind);

                var valueType = Interop.wasm_valtype_new(v.kind);
                var valueTypeHandle = valueType.DangerousGetHandle();
                valueType.SetHandleAsInvalid();

                using (var globalType = Interop.wasm_globaltype_new(
                    valueTypeHandle,
                    IsMutable ? Interop.wasm_mutability_t.WASM_VAR : Interop.wasm_mutability_t.WASM_CONST))
                {
                    Handle = Interop.wasm_global_new(store, globalType, &v);
                }
            }
        }

        public ValueKind Kind { get; private set; }

        public bool IsMutable { get; private set; }

        internal Interop.GlobalHandle Handle { get; set; }

        private T _initialValue;
    }
}