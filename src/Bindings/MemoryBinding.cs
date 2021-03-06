using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Wasmtime.Imports;

namespace Wasmtime.Bindings
{
    /// <summary>
    /// Represents a host memory binding.
    /// </summary>
    public class MemoryBinding : Binding
    {
        /// <summary>
        /// Constructs a new memory binding.
        /// </summary>
        /// <param name="import">The memory import of the binding.</param>
        /// <param name="field">The field the import is bound to.</param>
        public MemoryBinding(MemoryImport import, FieldInfo field)
        {
            if (import is null)
            {
                throw new ArgumentNullException(nameof(import));
            }

            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            Import = import;
            Field = field;

            Validate();
        }

        /// <summary>
        /// The memory import of the binding.
        /// </summary>
        public MemoryImport Import { get; private set; }

        /// <summary>
        /// The field the import is bound to.
        /// </summary>
        public FieldInfo Field { get; private set; }

        internal override SafeHandle Bind(Store store, IHost host)
        {
            dynamic memory = Field.GetValue(host);
            if (memory.Handle != null)
            {
                throw new InvalidOperationException("Cannot bind more than once.");
            }

            uint min = memory.Minimum;
            uint max = memory.Maximum;

            if (min != Import.Minimum)
            {
                ThrowBindingException(Import, Field, $"Memory does not have the expected minimum of {Import.Minimum} page(s)");
            }
            if (max != Import.Maximum)
            {
                ThrowBindingException(Import, Field, $"Memory does not have the expected maximum of {Import.Maximum} page(s)");
            }

            unsafe
            {
                Interop.wasm_limits_t limits = new Interop.wasm_limits_t();
                limits.min = min;
                limits.max = max;
                using (var memoryType = Interop.wasm_memorytype_new(&limits))
                {
                    var handle = Interop.wasm_memory_new(store.Handle, memoryType);
                    memory.Handle = handle;
                    return handle;
                }
            }
        }

        private void Validate()
        {
            if (Field.IsStatic)
            {
                ThrowBindingException(Import, Field, "field cannot be static");
            }

            if (!Field.IsInitOnly)
            {
                ThrowBindingException(Import, Field, "field must be readonly");
            }

            if (Field.FieldType != typeof(Memory))
            {
                ThrowBindingException(Import, Field, "field is expected to be of type 'Memory'");
            }
        }
    }
}