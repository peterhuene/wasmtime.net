using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Wasmtime;
using Xunit;

namespace Wasmtime.Tests
{
    public class GlobalExportsFixture : ModuleFixture
    {
        protected override string ModuleFileName => "GlobalExports.wasm";
    }

    public class GlobalExportsTests : IClassFixture<GlobalExportsFixture>
    {
        public GlobalExportsTests(GlobalExportsFixture fixture)
        {
            Fixture = fixture;
        }

        private GlobalExportsFixture Fixture { get; set; }

        [Theory]
        [MemberData(nameof(GetGlobalExports))]
        public void ItHasTheExpectedGlobalExports(string exportName, ValueKind expectedKind, bool expectedMutable)
        {
            var export = Fixture.Module.Exports.Globals.Where(f => f.Name == exportName).FirstOrDefault();
            export.Should().NotBeNull();
            export.Kind.Should().Be(expectedKind);
            export.IsMutable.Should().Be(expectedMutable);
        }

        [Fact]
        public void ItHasTheExpectedNumberOfExportedGlobals()
        {
            GetGlobalExports().Count().Should().Be(Fixture.Module.Exports.Globals.Count);
        }

        public static IEnumerable<object[]> GetGlobalExports()
        {
            yield return new object[] {
                "global_i32",
                ValueKind.Int32,
                false
            };

            yield return new object[] {
                "global_i32_mut",
                ValueKind.Int32,
                true
            };

            yield return new object[] {
                "global_i64",
                ValueKind.Int64,
                false
            };

            yield return new object[] {
                "global_i64_mut",
                ValueKind.Int64,
                true
            };

            yield return new object[] {
                "global_f32",
                ValueKind.Float32,
                false
            };

            yield return new object[] {
                "global_f32_mut",
                ValueKind.Float32,
                true
            };

            yield return new object[] {
                "global_f64",
                ValueKind.Float64,
                false
            };

            yield return new object[] {
                "global_f64_mut",
                ValueKind.Float64,
                true
            };
        }
    }
}
