using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Wasmtime;
using Xunit;

namespace Wasmtime.Tests
{
    public class GlobalImportBindingFixture : ModuleFixture
    {
        protected override string ModuleFileName => "GlobalImportBindings.wasm";
    }

    public class GlobalImportBindingTests : IClassFixture<GlobalImportBindingFixture>
    {
        class NoImportsHost : Wasmtime.Host
        {
        }

        class GlobalIsStaticHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public static int x = 0;
        }

        class GlobalIsNotReadOnlyHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public int x = 0;
        }

        class NotAGlobalHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly int x = 0;
        }

        class NotAValidGlobalTypeHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly Global<string> x = new Global<string>("nope");
        }

        class TypeMismatchHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly Global<long> x = new Global<long>(0);
        }

        class NotMutHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly Global<int> Int32Mut = new Global<int>(0);
        }

        class MutHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly Global<int> Int32Mut = new Global<int>(0, isMutable: true);

            [Import("global_i32")]
            public readonly Global<int> Int32 = new Global<int>(0, isMutable: true);
        }

        class ValidHost : Wasmtime.Host
        {
            [Import("global_i32_mut")]
            public readonly Global<int> Int32Mut = new Global<int>(0, isMutable: true);

            [Import("global_i32")]
            public readonly Global<int> Int32 = new Global<int>(1);

            [Import("global_i64_mut")]
            public readonly Global<long> Int64Mut = new Global<long>(2, isMutable: true);

            [Import("global_i64")]
            public readonly Global<long> Int64 = new Global<long>(3);

            [Import("global_f32_mut")]
            public readonly Global<float> Float32Mut = new Global<float>(4, isMutable: true);

            [Import("global_f32")]
            public readonly Global<float> Float32 = new Global<float>(5);

            [Import("global_f64_mut")]
            public readonly Global<double> Float64Mut = new Global<double>(6, isMutable: true);

            [Import("global_f64")]
            public readonly Global<double> Float64 = new Global<double>(7);
        }

        public GlobalImportBindingTests(GlobalImportBindingFixture fixture)
        {
            Fixture = fixture;
        }

        private GlobalImportBindingFixture Fixture { get; set; }

        [Fact]
        public void ItFailsToInstantiateWithMissingImport()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<NoImportsHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Failed to instantiate module 'GlobalImportBindings': missing global import *");
        }

        [Fact]
        public void ItFailsToInstantiateWithStaticField()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<GlobalIsStaticHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'GlobalIsStaticHost.x' to WebAssembly import 'global_i32_mut': field cannot be static.");
        }

        [Fact]
        public void ItFailsToInstantiateWithNonReadOnlyField()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<GlobalIsNotReadOnlyHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'GlobalIsNotReadOnlyHost.x' to WebAssembly import 'global_i32_mut': field must be readonly.");
        }

        [Fact]
        public void ItFailsToInstantiateWithInvalidType()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<NotAGlobalHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'NotAGlobalHost.x' to WebAssembly import 'global_i32_mut': field is expected to be of type 'Global<T>'.");
        }

        [Fact]
        public void ItFailsToInstantiateWithInvalidGlobalType()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<NotAValidGlobalTypeHost>()) { } };

            action
                .Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<NotSupportedException>("Type is expected to be 'int', 'long', 'float', or 'double'.");
        }

        [Fact]
        public void ItFailsToInstantiateWithGlobalTypeMismatch()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<TypeMismatchHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'TypeMismatchHost.x' to WebAssembly import 'global_i32_mut': global type argument is expected to be of type 'int'.");
        }

        [Fact]
        public void ItFailsToInstantiateWhenGlobalIsNotMut()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<NotMutHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'NotMutHost.Int32Mut' to WebAssembly import 'global_i32_mut': global is expected to be mutable.");
        }

        [Fact]
        public void ItFailsToInstantiateWhenGlobalIsMut()
        {
            Action action = () => { using (var instance = Fixture.Module.Instantiate<MutHost>()) { } };

            action
                .Should()
                .Throw<WasmtimeException>()
                .WithMessage("Unable to bind 'MutHost.Int32' to WebAssembly import 'global_i32': global is expected to be immutable.");
        }

        [Fact]
        public void ItBindsTheGlobalsCorrectly()
        {
            using (dynamic instance = Fixture.Module.Instantiate<ValidHost>())
            {
                var host = (ValidHost)instance.Host;

                host.Int32Mut.Value.Should().Be(0);
                ((int)instance.get_global_i32_mut()).Should().Be(0);
                host.Int32.Value.Should().Be(1);
                ((int)instance.get_global_i32()).Should().Be(1);
                host.Int64Mut.Value.Should().Be(2);
                ((long)instance.get_global_i64_mut()).Should().Be(2);
                host.Int64.Value.Should().Be(3);
                ((long)instance.get_global_i64()).Should().Be(3);
                host.Float32Mut.Value.Should().Be(4);
                ((float)instance.get_global_f32_mut()).Should().Be(4);
                host.Float32.Value.Should().Be(5);
                ((float)instance.get_global_f32()).Should().Be(5);
                host.Float64Mut.Value.Should().Be(6);
                ((double)instance.get_global_f64_mut()).Should().Be(6);
                host.Float64.Value.Should().Be(7);
                ((double)instance.get_global_f64()).Should().Be(7);

                host.Int32Mut.Value = 10;
                host.Int32Mut.Value.Should().Be(10);
                ((int)instance.get_global_i32_mut()).Should().Be(10);
                instance.set_global_i32_mut(11);
                host.Int32Mut.Value.Should().Be(11);
                ((int)instance.get_global_i32_mut()).Should().Be(11);

                host.Int64Mut.Value = 12;
                host.Int64Mut.Value.Should().Be(12);
                ((long)instance.get_global_i64_mut()).Should().Be(12);
                instance.set_global_i64_mut(13);
                host.Int64Mut.Value.Should().Be(13);
                ((long)instance.get_global_i64_mut()).Should().Be(13);

                host.Float32Mut.Value = 14;
                host.Float32Mut.Value.Should().Be(14);
                ((float)instance.get_global_f32_mut()).Should().Be(14);
                instance.set_global_f32_mut(15);
                host.Float32Mut.Value.Should().Be(15);
                ((float)instance.get_global_f32_mut()).Should().Be(15);

                host.Float64Mut.Value = 16;
                host.Float64Mut.Value.Should().Be(16);
                ((double)instance.get_global_f64_mut()).Should().Be(16);
                instance.set_global_f64_mut(17);
                host.Float64Mut.Value.Should().Be(17);
                ((double)instance.get_global_f64_mut()).Should().Be(17);

                Action action = () => host.Int32.Value = 0;
                action
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("The value of the global cannot be modified.");

                action = () => host.Int64.Value = 0;
                action
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("The value of the global cannot be modified.");

                action = () => host.Float32.Value = 0;
                action
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("The value of the global cannot be modified.");

                action = () => host.Float64.Value = 0;
                action
                    .Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage("The value of the global cannot be modified.");
            }
        }
    }
}
