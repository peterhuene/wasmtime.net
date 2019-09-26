using System;
using System.IO;
using Wasmtime;

namespace Wasmtime.Tests
{
    public abstract class ModuleFixture : IDisposable
    {
        public ModuleFixture()
        {
            Engine = new Engine();
            Store = Engine.CreateStore();
            Module = Store.CreateModule(Path.Combine("modules", ModuleFileName));
        }

        public void Dispose()
        {
        }

        public Engine Engine { get; set; }
        public Store Store { get; set; }
        public Module Module { get; set; }

        protected abstract string ModuleFileName { get; }
    }
}