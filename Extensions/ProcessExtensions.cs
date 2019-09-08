using ProcessDump.Win32;
using System;
using System.Diagnostics;

namespace ProcessDump.Extensions
{
    public static class ProcessExtensions
    {
        public static bool Is32BitProcess(this Process proc)
        {
            bool fIs32bit = false;

            // if we're runing on 32bit, default to true
            if (IntPtr.Size == 4)
            {
                fIs32bit = true;
            }

            // if machine is 32 bit then all procs are 32 bit
            if (NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out bool fIsRunningUnderWow64)
                && fIsRunningUnderWow64)
            {
                // current OS is 64 bit
                if (NativeMethods.IsWow64Process(proc.Handle, out fIsRunningUnderWow64)
                      && fIsRunningUnderWow64)
                {
                    fIs32bit = true;
                }
                else
                {
                    fIs32bit = false;
                }
            }

            return fIs32bit;
        }
    }
}
