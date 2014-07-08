using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Wrapper
{
    public static class NativeMethods
    {
        [DllImport("kernel32")]
        public static extern bool AllocConsole();
    }
}
