using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Wasmtime
{
    /// <summary>
    /// Implements the Wasmtime API bindings.
    /// </summary>
    /// <remarks>See https://github.com/WebAssembly/wasm-c-api/blob/master/include/wasm.h for the C API reference.</remarks>
    internal static class Interop
    {
        internal class WasmtimeEngineHandle : SafeHandle
        {
            public WasmtimeEngineHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_engine_delete(handle);
                return true;
            }
        }

        internal class WasmtimeStoreHandle : SafeHandle
        {
            public WasmtimeStoreHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_store_delete(handle);
                return true;
            }
        }

        internal class WasmtimeModuleHandle : SafeHandle
        {
            public WasmtimeModuleHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_module_delete(handle);
                return true;
            }
        }

        internal class WasmtimeFunctionHandle : SafeHandle
        {
            public WasmtimeFunctionHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_func_delete(handle);
                return true;
            }
        }

        internal class WasmtimeInstanceHandle : SafeHandle
        {
            public WasmtimeInstanceHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_instance_delete(handle);
                return true;
            }
        }

        internal class WasmtimeFuncTypeHandle : SafeHandle
        {
            public WasmtimeFuncTypeHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_functype_delete(handle);
                return true;
            }
        }

        internal class WasmtimeValueTypeHandle : SafeHandle
        {
            public WasmtimeValueTypeHandle() : base(IntPtr.Zero, true)
            {
            }

            public override bool IsInvalid => handle == IntPtr.Zero;

            protected override bool ReleaseHandle()
            {
                Interop.wasm_valtype_delete(handle);
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_byte_vec_t
        {
            public UIntPtr size;
            public byte* data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_valtype_vec_t
        {
            public UIntPtr size;
            public IntPtr* data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_export_vec_t
        {
            public UIntPtr size;
            public IntPtr* data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_extern_vec_t
        {
            public UIntPtr size;
            public IntPtr* data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_importtype_vec_t
        {
            public UIntPtr size;
            public IntPtr* data;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct wasm_exporttype_vec_t
        {
            public UIntPtr size;
            public IntPtr* data;
        }

        internal enum wasm_valkind_t : byte
        {
            WASM_I32,
            WASM_I64,
            WASM_F32,
            WASM_F64,
            WASM_ANYREF = 128,
            WASM_FUNCREF,
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct wasm_val_union_t
        {
            [FieldOffset(0)]
            public int i32;

            [FieldOffset(0)]
            public long i64;

            [FieldOffset(0)]
            public float f32;

            [FieldOffset(0)]
            public double f64;

            [FieldOffset(0)]
            public IntPtr reference;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct wasm_val_t
        {
            public wasm_valkind_t kind;
            public wasm_val_union_t of;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct wasm_limits_t
        {
            public uint min;

            public uint max;
        }

        internal static wasm_val_t ToValue(object o)
        {
            wasm_val_t value = new wasm_val_t();
            switch (o)
            {
                case int i:
                    value.kind = wasm_valkind_t.WASM_I32;
                    value.of.i32 = i;
                    break;

                case long i:
                    value.kind = wasm_valkind_t.WASM_I64;
                    value.of.i64 = i;
                    break;

                case float f:
                    value.kind = wasm_valkind_t.WASM_F32;
                    value.of.f32 = f;
                    break;

                case double d:
                    value.kind = wasm_valkind_t.WASM_F64;
                    value.of.f64 = d;
                    break;

                // TODO: support AnyRef

                default:
                    throw new NotSupportedException("Unsupported value type.");
            }
            return value;
        }

        internal static object ToObject(wasm_val_t v)
        {
            switch (v.kind)
            {
                case Interop.wasm_valkind_t.WASM_I32:
                    return v.of.i32;

                case Interop.wasm_valkind_t.WASM_I64:
                    return v.of.i64;

                case Interop.wasm_valkind_t.WASM_F32:
                    return v.of.f32;

                case Interop.wasm_valkind_t.WASM_F64:
                    return v.of.f64;

                // TODO: support AnyRef

                default:
                    throw new NotSupportedException("Unsupported value kind.");
            }
        }

        internal unsafe delegate IntPtr WasmFuncCallback(wasm_val_t* parameters, wasm_val_t* results);

        internal enum wasm_externkind_t : byte
        {
            WASM_EXTERN_FUNC,
            WASM_EXTERN_GLOBAL,
            WASM_EXTERN_TABLE,
            WASM_EXTERN_MEMORY,
        }

        internal enum wasm_mutability_t : byte
        {
            WASM_CONST,
            WASM_VAR,
        }

        internal static unsafe List<ValueKind> ToValueKindList(Interop.wasm_valtype_vec_t* vec)
        {
            var list = new List<ValueKind>((int)vec->size);

            for (int i = 0; i < (int)vec->size; ++i)
            {
                list.Add(Interop.wasm_valtype_kind(vec->data[i]));
            }

            return list;
        }

        internal static Interop.wasm_valtype_vec_t ToValueTypeVec(IReadOnlyList<ValueKind> collection)
        {
            Interop.wasm_valtype_vec_t vec;
            Interop.wasm_valtype_vec_new_uninitialized(out vec, (UIntPtr)collection.Count);

            int i = 0;
            foreach (var type in collection)
            {
                var valType = Interop.wasm_valtype_new(type);
                unsafe
                {
                    vec.data[i++] = valType.DangerousGetHandle();
                }
                valType.SetHandleAsInvalid();
            }

            return vec;
        }

        // Engine imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeEngineHandle wasm_engine_new();

        [DllImport("wasmtime_api")]
        public static extern void wasm_engine_delete(IntPtr engine);

        // Store imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeStoreHandle wasm_store_new(WasmtimeEngineHandle engine);

        [DllImport("wasmtime_api")]
        public static extern void wasm_store_delete(IntPtr engine);

        // Byte vec imports

        [DllImport("wasmtime_api")]
        public static extern void wasm_byte_vec_new_empty(out wasm_byte_vec_t vec);

        [DllImport("wasmtime_api")]
        public static extern void wasm_byte_vec_new_uninitialized(out wasm_byte_vec_t vec, UIntPtr length);

        [DllImport("wasmtime_api")]
        public static extern void wasm_byte_vec_new(out wasm_byte_vec_t vec, UIntPtr length, byte[] data);

        [DllImport("wasmtime_api")]
        public static extern void wasm_byte_vec_copy(out wasm_byte_vec_t vec, ref wasm_byte_vec_t src);

        [DllImport("wasmtime_api")]
        public static extern void wasm_byte_vec_delete(ref wasm_byte_vec_t vec);

        // Value type vec imports

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_vec_new_empty(out wasm_valtype_vec_t vec);

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_vec_new_uninitialized(out wasm_valtype_vec_t vec, UIntPtr length);

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_vec_new(out wasm_valtype_vec_t vec, UIntPtr length, IntPtr[] data);

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_vec_copy(out wasm_valtype_vec_t vec, ref wasm_valtype_vec_t src);

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_vec_delete(ref wasm_valtype_vec_t vec);

        // Extern vec imports

        [DllImport("wasmtime_api")]
        public static extern void wasm_extern_vec_new_empty(out wasm_extern_vec_t vec);

        [DllImport("wasmtime_api")]
        public static extern void wasm_extern_vec_new_uninitialized(out wasm_extern_vec_t vec, UIntPtr length);

        [DllImport("wasmtime_api")]
        public static extern void wasm_extern_vec_new(out wasm_extern_vec_t vec, UIntPtr length, IntPtr[] data);

        [DllImport("wasmtime_api")]
        public static extern void wasm_extern_vec_copy(out wasm_extern_vec_t vec, ref wasm_extern_vec_t src);

        [DllImport("wasmtime_api")]
        public static extern void wasm_extern_vec_delete(ref wasm_extern_vec_t vec);

        // Import type vec imports

        [DllImport("wasmtime_api")]
        public static extern void wasm_importtype_vec_new_empty(out wasm_importtype_vec_t vec);

        [DllImport("wasmtime_api")]
        public static extern void wasm_importtype_vec_new_uninitialized(out wasm_importtype_vec_t vec, UIntPtr length);

        [DllImport("wasmtime_api")]
        public static extern void wasm_importtype_vec_new(out wasm_importtype_vec_t vec, UIntPtr length, IntPtr[] data);

        [DllImport("wasmtime_api")]
        public static extern void wasm_importtype_vec_copy(out wasm_importtype_vec_t vec, ref wasm_importtype_vec_t src);

        [DllImport("wasmtime_api")]
        public static extern void wasm_importtype_vec_delete(ref wasm_importtype_vec_t vec);

        // Export type vec imports

        [DllImport("wasmtime_api")]
        public static extern void wasm_exporttype_vec_new_empty(out wasm_exporttype_vec_t vec);

        [DllImport("wasmtime_api")]
        public static extern void wasm_exporttype_vec_new_uninitialized(out wasm_exporttype_vec_t vec, UIntPtr length);

        [DllImport("wasmtime_api")]
        public static extern void wasm_exporttype_vec_new(out wasm_exporttype_vec_t vec, UIntPtr length, IntPtr[] data);

        [DllImport("wasmtime_api")]
        public static extern void wasm_exporttype_vec_copy(out wasm_exporttype_vec_t vec, ref wasm_exporttype_vec_t src);

        [DllImport("wasmtime_api")]
        public static extern void wasm_exporttype_vec_delete(ref wasm_exporttype_vec_t vec);

        // Import type imports

        [DllImport("wasmtime_api")]
        public static extern unsafe wasm_byte_vec_t* wasm_importtype_module(IntPtr importType);

        [DllImport("wasmtime_api")]
        public static extern unsafe wasm_byte_vec_t* wasm_importtype_name(IntPtr importType);

        [DllImport("wasmtime_api")]
        public static extern unsafe IntPtr wasm_importtype_type(IntPtr importType);

        // Export type imports

        [DllImport("wasmtime_api")]
        public static extern unsafe wasm_byte_vec_t* wasm_exporttype_name(IntPtr exportType);

        [DllImport("wasmtime_api")]
        public static extern unsafe IntPtr wasm_exporttype_type(IntPtr exportType);

        // Module imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeModuleHandle wasm_module_new(WasmtimeStoreHandle store, ref wasm_byte_vec_t bytes);

        [DllImport("wasmtime_api")]
        public static extern void wasm_module_imports(WasmtimeModuleHandle module, out wasm_importtype_vec_t imports);

        [DllImport("wasmtime_api")]
        public static extern void wasm_module_exports(WasmtimeModuleHandle module, out wasm_exporttype_vec_t exports);

        [DllImport("wasmtime_api")]
        public static extern void wasm_module_delete(IntPtr module);

        // Value type imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeValueTypeHandle wasm_valtype_new(ValueKind kind);

        [DllImport("wasmtime_api")]
        public static extern void wasm_valtype_delete(IntPtr valueType);

        [DllImport("wasmtime_api")]
        public static extern ValueKind wasm_valtype_kind(IntPtr valueType);

        // Extern imports

        [DllImport("wasmtime_api")]
        public static extern wasm_externkind_t wasm_extern_kind(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_extern_type(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_extern_as_func(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_extern_as_global(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_extern_as_table(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_extern_as_memory(IntPtr ext);

        // Extern type imports

        [DllImport("wasmtime_api")]
        public static extern wasm_externkind_t wasm_externtype_kind(IntPtr externType);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_externtype_as_functype_const(IntPtr externType);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_externtype_as_globaltype_const(IntPtr externType);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_externtype_as_tabletype_const(IntPtr externType);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_externtype_as_memorytype_const(IntPtr externType);

        // Function imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeFunctionHandle wasm_func_new(WasmtimeStoreHandle store, WasmtimeFuncTypeHandle type, WasmFuncCallback callback);

        [DllImport("wasmtime_api")]
        public static extern void wasm_func_delete(IntPtr function);

        [DllImport("wasmtime_api")]
        public static unsafe extern IntPtr wasm_func_call(IntPtr function, wasm_val_t* args, wasm_val_t* results);

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_func_as_extern(WasmtimeFunctionHandle function);

        // Function type imports

        [DllImport("wasmtime_api")]
        public static extern unsafe wasm_valtype_vec_t* wasm_functype_params(IntPtr funcType);

        [DllImport("wasmtime_api")]
        public static extern unsafe wasm_valtype_vec_t* wasm_functype_results(IntPtr funcType);

        // Instance imports

        [DllImport("wasmtime_api")]
        public static extern unsafe WasmtimeInstanceHandle wasm_instance_new(WasmtimeStoreHandle store, WasmtimeModuleHandle module, IntPtr[] imports, out IntPtr trap);

        [DllImport("wasmtime_api")]
        public static extern void wasm_instance_delete(IntPtr ext);

        [DllImport("wasmtime_api")]
        public static extern void wasm_instance_exports(WasmtimeInstanceHandle instance, out wasm_extern_vec_t exports);

        // Function type imports

        [DllImport("wasmtime_api")]
        public static extern WasmtimeFuncTypeHandle wasm_functype_new(ref wasm_valtype_vec_t parameters, ref wasm_valtype_vec_t results);

        [DllImport("wasmtime_api")]
        public static extern void wasm_functype_delete(IntPtr functype);

        // Global type imports

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_globaltype_content(IntPtr globalType);

        [DllImport("wasmtime_api")]
        public static extern wasm_mutability_t wasm_globaltype_mutability(IntPtr globalType);

        // Trap imports

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_trap_new(WasmtimeStoreHandle store, ref wasm_byte_vec_t message);

        [DllImport("wasmtime_api")]
        public static extern void wasm_trap_delete(IntPtr trap);

        [DllImport("wasmtime_api")]
        public static extern void wasm_trap_message(IntPtr trap, out wasm_byte_vec_t message);

        // Table type imports

        [DllImport("wasmtime_api")]
        public static extern IntPtr wasm_tabletype_element(IntPtr tableType);

        [DllImport("wasmtime_api")]
        public static unsafe extern wasm_limits_t* wasm_tabletype_limits(IntPtr tableType);
    }
}
