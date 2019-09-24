using System;
using System.Collections.Generic;

namespace Wasmtime
{
    /// <summary>
    /// Represents external (instantiated) WASM functions, globals, tables, and memories.
    /// </summary>
    public class Externs
    {
        internal Externs(Exports exports, Interop.wasm_extern_vec_t externs)
        {
            var functions = new List<ExternFunction>();

            for (int i = 0; i < (int)externs.size; ++i)
            {
                unsafe
                {
                    var ext = externs.data[i];

                    switch (Interop.wasm_extern_kind(ext))
                    {
                        case Interop.wasm_externkind_t.WASM_EXTERN_FUNC:
                            var function = new ExternFunction((FunctionExport)exports.All[i], Interop.wasm_extern_as_func(ext));
                            functions.Add(function);
                            break;

                        default:
                            throw new NotSupportedException("Unsupported extern type.");
                    }
                }
            }

            Functions = functions;
        }

        /// <summary>
        /// The extern functions from an instantiated WASM module.
        /// </summary>
        public IReadOnlyList<ExternFunction> Functions { get; private set; }
    }
}