using System;
using Wasmtime;

namespace HelloExample
{
    class Host : Wasmtime.Host
    {
        [Import("hello")]
        public void SayHello()
        {
            Console.WriteLine("Hello from C#, WASM!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var engine = new Engine())
            {
                using (var store = engine.CreateStore())
                {
                    using (var module = store.CreateModule("hello.wasm"))
                    {
                        using (dynamic instance = module.Instantiate<Host>())
                        {
                            instance.run();
                        }
                    }
                }
            }
        }
    }
}
