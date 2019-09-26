using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Wasmtime;
using Xunit;

namespace Wasmtime.Tests
{
    public class FunctionExportsFixture : ModuleFixture
    {
        protected override string ModuleFileName => "FunctionExports.wasm";
    }

    public class FunctionExports : IClassFixture<FunctionExportsFixture>
    {
        public FunctionExports(FunctionExportsFixture fixture)
        {
            Fixture = fixture;
        }

        private FunctionExportsFixture Fixture { get; set; }

        [Theory]
        [MemberData(nameof(GetFunctionExports))]
        public void ItHasTheExpectedFunctionExports(string exportName, ValueKind[] expectedParameters, ValueKind[] expectedResults)
        {
            var export = Fixture.Module.Exports.Functions.Where(f => f.Name == exportName).FirstOrDefault();
            export.Should().NotBeNull();
            export.Parameters.Should().Equal(expectedParameters);
            export.Results.Should().Equal(expectedResults);
        }

        [Fact]
        public void ItHasTheExpectedNumberOfExportedFunctions()
        {
            GetFunctionExports().Count().Should().Be(Fixture.Module.Exports.Functions.Count);
        }

        public static IEnumerable<object[]> GetFunctionExports()
        {
            yield return new object[] {
                "no_params_no_results",
                new ValueKind[0],
                new ValueKind[0]
            };

            yield return new object[] {
                "one_i32_param_no_results",
                new ValueKind[] {
                    ValueKind.Int32
                },
                new ValueKind[0]
            };

            yield return new object[] {
                "one_i64_param_no_results",
                new ValueKind[] {
                    ValueKind.Int64
                },
                new ValueKind[0]
            };

            yield return new object[] {
                "one_f32_param_no_results",
                new ValueKind[] {
                    ValueKind.Float32
                },
                new ValueKind[0]
            };

            yield return new object[] {
                "one_f64_param_no_results",
                new ValueKind[] {
                    ValueKind.Float64
                },
                new ValueKind[0]
            };

            yield return new object[] {
                "one_param_of_each_type",
                new ValueKind[] {
                    ValueKind.Int32,
                    ValueKind.Int64,
                    ValueKind.Float32,
                    ValueKind.Float64
                },
                new ValueKind[0]
            };

            yield return new object[] {
                "no_params_one_i32_result",
                new ValueKind[0],
                new ValueKind[] {
                    ValueKind.Int32,
                }
            };

            yield return new object[] {
                "no_params_one_i64_result",
                new ValueKind[0],
                new ValueKind[] {
                    ValueKind.Int64,
                }
            };

            yield return new object[] {
                "no_params_one_f32_result",
                new ValueKind[0],
                new ValueKind[] {
                    ValueKind.Float32,
                }
            };

            yield return new object[] {
                "no_params_one_f64_result",
                new ValueKind[0],
                new ValueKind[] {
                    ValueKind.Float64,
                }
            };

            yield return new object[] {
                "one_result_of_each_type",
                new ValueKind[0],
                new ValueKind[] {
                    ValueKind.Int32,
                    ValueKind.Int64,
                    ValueKind.Float32,
                    ValueKind.Float64,
                }
            };

            yield return new object[] {
                "one_param_and_result_of_each_type",
                new ValueKind[] {
                    ValueKind.Int32,
                    ValueKind.Int64,
                    ValueKind.Float32,
                    ValueKind.Float64,
                },
                new ValueKind[] {
                    ValueKind.Int32,
                    ValueKind.Int64,
                    ValueKind.Float32,
                    ValueKind.Float64,
                }
            };
        }
    }
}
