using System;
using Wasmtime;

namespace HelloExample
{
    class Host : Wasmtime.Host
    {
        [Import("print_global")]
        public void PrintGlobal()
        {
            Console.WriteLine($"The value of the global is: {Global.Value}.");
        }

        [Import("global")]
        public readonly Global<int> Global = new Global<int>(1, isMutable: true);
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = new Engine())
            {
                using (var store = engine.CreateStore())
                {
                    using (var module = store.CreateModule("global.wasm"))
                    {
                        using (dynamic instance = module.Instantiate<Host>())
                        {
                            instance.run(20);
                        }
                    }
                }
            }
        }
    }
}
