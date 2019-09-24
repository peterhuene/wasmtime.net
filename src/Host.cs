using System;
using System.Collections.Generic;

namespace Wasmtime
{
    /// <summary>
    /// The base class for host imports.
    /// </summary>
    public class Host
    {
        /// <summary>
        /// The <see href="Instance" /> associated with the host.
        /// </summary>
        public Instance Instance { get; internal set; }
    }
}