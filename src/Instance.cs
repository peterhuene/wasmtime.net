using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Dynamic;
using System.Text;

namespace Wasmtime
{
    /// <summary>
    /// Represents an instantiated WebAssembly module.
    /// </summary>
    public class Instance : DynamicObject, IDisposable
    {
        internal Instance(Module module, Host host)
        {
            Module = module;

            var bindings = BindImports(host);

            unsafe
            {
                Handle = Interop.wasm_instance_new(
                    Module.Store.Handle,
                    Module.Handle,
                    bindings.Select(b => ToExtern(b)).ToArray(),
                    out var trap);

                if (trap != IntPtr.Zero)
                {
                    throw TrapException.FromOwnedTrap(trap);
                }

                bindings.ForEach(f => f.Dispose());
            }

            if (Handle.IsInvalid)
            {
                throw new WasmtimeException($"Failed to instantiate module '{module.Name}'.");
            }

            Interop.wasm_instance_exports(Handle, out _externs);

            Externs = new Externs(Module.Exports, _externs);

            _functions = Externs.Functions.ToDictionary(f => f.Name);
        }

        /// <summary>
        /// The WebAssembly module associated with the instantiation.
        /// </summary>
        public Module Module { get; private set; }

        /// <summary>
        /// The external (instantiated) collection of functions, globals, tables, and memories.
        /// </summary>
        public Externs Externs { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!Handle.IsInvalid)
            {
                Handle.Dispose();
                Handle.SetHandleAsInvalid();
            }
            if (_externs.size != UIntPtr.Zero)
            {
                Interop.wasm_extern_vec_delete(ref _externs);
                _externs.size = UIntPtr.Zero;
            }
        }

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (!_functions.TryGetValue(binder.Name, out var func))
            {
                result = null;
                return false;
            }

            result = func.Invoke(args);
            return true;
        }

        private List<SafeHandle> BindImports(Host host)
        {
            var type = host.GetType();
            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            var bindings = new List<SafeHandle>();
            foreach (var import in Module.Imports.All)
            {
                switch (import)
                {
                    case FunctionImport f:
                        bindings.Add(BindImportFunction(host, methods, f));
                        break;

                    default:
                        throw new NotSupportedException("Unsupported import binding type.");
                }
            }
            return bindings;
        }

        private Interop.WasmtimeFunctionHandle BindImportFunction(Host host, IEnumerable<MethodInfo> methods, FunctionImport import)
        {
            var method = methods.Where(m =>
            {
                var attribute = (ImportAttribute)m.GetCustomAttribute(typeof(ImportAttribute));
                if (attribute == null)
                {
                    return false;
                }

                return attribute.Name == import.Name &&
                        ((string.IsNullOrEmpty(attribute.Module) &&
                        string.IsNullOrEmpty(import.Module)) ||
                        attribute.Module == import.Module);
            }).FirstOrDefault();

            if (method == null)
            {
                throw new WasmtimeException($"Failed to instantiate module '{Module.Name}': missing import '{import}'.");
            }

            ValidateImportFunction(import, method);

            var callback = RegisterCallback(host, method, import.Parameters.Count);

            unsafe
            {
                var parameters = Interop.ToValueTypeVec(import.Parameters);
                var results = Interop.ToValueTypeVec(import.Results);
                using (var funcType = Interop.wasm_functype_new(ref parameters, ref results))
                {
                    return Interop.wasm_func_new(Module.Store.Handle, funcType, callback);
                }
            }
        }

        private static void ThrowBindingException(FunctionImport import, MethodInfo method, string message)
        {
            throw new WasmtimeException($"Unable to bind method '{method.DeclaringType.Name}.{method.Name}' to WebAssembly import '{import}': {message}.");
        }

        private static void ValidateImportFunction(FunctionImport import, MethodInfo method)
        {
            if (method.IsStatic)
            {
                ThrowBindingException(import, method, "method cannot be static");
            }

            if (method.IsGenericMethod)
            {
                ThrowBindingException(import, method, "method cannot be generic");
            }

            if (method.IsConstructor)
            {
                ThrowBindingException(import, method, "method cannot be a constructor");
            }

            ValidateParameters(import, method);

            ValidateReturnType(import, method);
        }

        private static void ValidateParameters(FunctionImport import, MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != import.Parameters.Count)
            {
                ThrowBindingException(
                    import,
                    method,
                    $"parameter mismatch: import requires {import.Parameters.Count} but the method has {parameters.Length}");
            }

            for (int i = 0; i < parameters.Length; ++i)
            {
                var parameter = parameters[i];
                if (parameter.ParameterType.IsByRef)
                {
                    if (parameter.IsOut)
                    {
                        ThrowBindingException(import, method, $"parameter '{parameter.Name}' cannot be an 'out' parameter");
                    }
                    else
                    {
                        ThrowBindingException(import, method, $"parameter '{parameter.Name}' cannot be a 'ref' parameter");
                    }
                }

                var expected = import.Parameters[i];
                if (!TryGetValueKind(parameter.ParameterType, out var kind) || kind != expected)
                {
                    ThrowBindingException(import, method, $"method parameter '{parameter.Name}' is expected to be of type '{ToString(expected)}'");
                }
            }
        }

        private static void ValidateReturnType(FunctionImport import, MethodInfo method)
        {
            int resultsCount = import.Results.Count();
            if (resultsCount == 0)
            {
                if (method.ReturnType != typeof(void))
                {
                    ThrowBindingException(import, method, "method must return void");
                }
            }
            else if (resultsCount == 1)
            {
                var expected = import.Results[0];
                if (!TryGetValueKind(method.ReturnType, out var kind) || kind != expected)
                {
                    ThrowBindingException(import, method, $"return type is expected to be '{ToString(expected)}'");
                }
            }
            else
            {
                if (!IsTupleOfSize(method.ReturnType, resultsCount))
                {
                    ThrowBindingException(import, method, $"return type is expected to be a tuple of size {resultsCount}");
                }

                var typeArguments =
                    method.ReturnType.GetGenericArguments().SelectMany(type =>
                    {
                        if (type.IsConstructedGenericType)
                        {
                            return type.GenericTypeArguments;
                        }
                        return Enumerable.Repeat(type, 1);
                    });

                int i = 0;
                foreach (var typeArgument in typeArguments)
                {
                    var expected = import.Results[i];
                    if (!TryGetValueKind(typeArgument, out var kind) || kind != expected)
                    {
                        ThrowBindingException(import, method, $"return tuple item #{i} is expected to be of type '{ToString(expected)}'");
                    }

                    ++i;
                }
            }
        }

        private unsafe Interop.WasmFuncCallback RegisterCallback(Host host, MethodInfo method, int argCount)
        {
            var args = new object[argCount];
            bool hasReturn = method.ReturnType != typeof(void);
            var store = Module.Store.Handle;

            Interop.WasmFuncCallback callback = (arguments, results) =>
            {
                try
                {
                    SetArgs(arguments, args);

                    var result = method.Invoke(host, args);

                    if (hasReturn)
                    {
                        SetResults(result, results);
                    }
                    return IntPtr.Zero;
                }
                catch (TargetInvocationException ex)
                {
                    var bytes = Encoding.UTF8.GetBytes(ex.InnerException.Message + "\0" /* exception messages need a null */);

                    fixed (byte* ptr = bytes)
                    {
                        Interop.wasm_byte_vec_t message = new Interop.wasm_byte_vec_t();
                        message.size = (UIntPtr)bytes.Length;
                        message.data = ptr;

                        return Interop.wasm_trap_new(store, ref message);
                    }
                }
            };

            _callbacks.Add(callback);
            return callback;
        }

        private static unsafe void SetArgs(Interop.wasm_val_t* arguments, object[] args)
        {
            for (int i = 0; i < args.Length; ++i)
            {
                var arg = arguments[i];

                switch (arg.kind)
                {
                    case Interop.wasm_valkind_t.WASM_I32:
                        args[i] = arg.of.i32;
                        break;

                    case Interop.wasm_valkind_t.WASM_I64:
                        args[i] = arg.of.i64;
                        break;

                    case Interop.wasm_valkind_t.WASM_F32:
                        args[i] = arg.of.f32;
                        break;

                    case Interop.wasm_valkind_t.WASM_F64:
                        args[i] = arg.of.f64;
                        break;

                    default:
                        throw new NotSupportedException("Unsupported value type.");
                }
            }
        }

        private static unsafe void SetResults(object value, Interop.wasm_val_t* results)
        {
            var tuple = value as ITuple;
            if (tuple is null)
            {
                SetResult(value, &results[0]);
            }
            else
            {
                for (int i = 0; i < tuple.Length; ++i)
                {
                    SetResults(tuple[i], &results[i]);
                }
            }
        }

        private static unsafe void SetResult(object value, Interop.wasm_val_t* result)
        {
            switch (value)
            {
                case int i:
                    result->kind = Interop.wasm_valkind_t.WASM_I32;
                    result->of.i32 = i;
                    break;

                case long l:
                    result->kind = Interop.wasm_valkind_t.WASM_I64;
                    result->of.i64 = l;
                    break;

                case float f:
                    result->kind = Interop.wasm_valkind_t.WASM_F32;
                    result->of.f32 = f;
                    break;

                case double d:
                    result->kind = Interop.wasm_valkind_t.WASM_F64;
                    result->of.f64 = d;
                    break;

                default:
                    throw new NotSupportedException("Unsupported return value type.");
            }
        }

        private static unsafe IntPtr ToExtern(SafeHandle handle)
        {
            switch (handle)
            {
                case Interop.WasmtimeFunctionHandle f:
                    return Interop.wasm_func_as_extern(f);

                default:
                    throw new NotSupportedException("Unexpected handle type.");
            }
        }

        private static bool TryGetValueKind(Type type, out ValueKind kind)
        {
            if (type == typeof(int))
            {
                kind = ValueKind.Int32;
                return true;
            }

            if (type == typeof(long))
            {
                kind = ValueKind.Int64;
                return true;
            }

            if (type == typeof(float))
            {
                kind = ValueKind.Float32;
                return true;
            }

            if (type == typeof(double))
            {
                kind = ValueKind.Float64;
                return true;
            }

            kind = default(ValueKind);
            return false;
        }

        private static string ToString(ValueKind kind)
        {
            switch (kind)
            {
                case ValueKind.Int32:
                    return "int";

                case ValueKind.Int64:
                    return "long";

                case ValueKind.Float32:
                    return "float";

                case ValueKind.Float64:
                    return "double";

                default:
                    throw new NotSupportedException("Unsupported value kind.");
            }
        }

        private static bool IsTupleOfSize(Type type, int size)
        {
            if (!type.IsConstructedGenericType)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();

            if (size == 0)
            {
                return definition == typeof(ValueTuple);
            }

            if (size == 1)
            {
                return definition == typeof(ValueTuple<>);
            }

            if (size == 2)
            {
                return definition == typeof(ValueTuple<,>);
            }

            if (size == 3)
            {
                return definition == typeof(ValueTuple<,,>);
            }

            if (size == 4)
            {
                return definition == typeof(ValueTuple<,,,>);
            }

            if (size == 5)
            {
                return definition == typeof(ValueTuple<,,,,>);
            }

            if (size == 6)
            {
                return definition == typeof(ValueTuple<,,,,,>);
            }

            if (size == 7)
            {
                return definition == typeof(ValueTuple<,,,,,,>);
            }

            if (definition != typeof(ValueTuple<,,,,,,,>))
            {
                return false;
            }

            return IsTupleOfSize(type.GetGenericArguments().Last(), size - 7);
        }

        internal Interop.WasmtimeInstanceHandle Handle { get; private set; }
        private List<Interop.WasmFuncCallback> _callbacks = new List<Interop.WasmFuncCallback>();
        private Interop.wasm_extern_vec_t _externs;
        private Dictionary<string, ExternFunction> _functions;
    }
}
