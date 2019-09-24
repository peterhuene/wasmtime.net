using System;
using System.Diagnostics;

namespace Wasmtime
{
    /// <summary>
    /// Represents a table imported to a WASM module.
    /// </summary>
    public class TableImport : Import
    {
        internal TableImport(IntPtr importType, IntPtr externType) : base(importType)
        {
            Debug.Assert(Interop.wasm_externtype_kind(externType) == Interop.wasm_externkind_t.WASM_EXTERN_TABLE);

            var tableType = Interop.wasm_externtype_as_tabletype_const(externType);
        }

        /// <summary>
        /// The value kind of the table.
        /// </summary>
        public ValueKind Kind { get; private set; }

        /// <summary>
        /// The minimum number of elements in the table.
        /// </summary>
        public int Minimum { get; private set; }

        /// <summary>
        /// The maximum number of elements in the table.
        /// </summary>
        public int Maximum { get; private set; }
    }
}