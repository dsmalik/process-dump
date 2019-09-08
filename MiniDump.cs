using ProcessDump.Extensions;
using ProcessDump.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProcessDump
{
    public class MiniDump
    {
        public static string Create(string processName)
        {
            return Create(processName, NativeMethods._MINIDUMP_TYPE.MiniDumpNormal, $@"c:\temp");
        }

        public static string Create(string processName, NativeMethods._MINIDUMP_TYPE dumpType)
        {
            return Create(processName, dumpType, $@"c:\temp");
        }

        public static string Create(string processName, NativeMethods._MINIDUMP_TYPE dumpType, string outputPath)
        {
            IntPtr hFile = IntPtr.Zero;
            if (IntPtr.Size == 4)
            {
                Console.WriteLine($"CreateMiniDump {dumpType} - Running as 32 bit, creating 32 bit dumps");
            }
            else
            {
                Console.WriteLine($"CreateMiniDump {dumpType} - Running as 64 bit, creating 64 bit dumps");
            }

            try
            {
                var dumpFileName = $@"{outputPath}\MiniDumpProcess-{dumpType.ToString()}.dmp";
                if (System.IO.File.Exists(dumpFileName))
                {
                    System.IO.File.Delete(dumpFileName);
                }

                hFile = NativeMethods.CreateFile(dumpFileName, NativeMethods.EFileAccess.GenericWrite,
                  NativeMethods.EFileShare.None, lpSecurityAttributes: IntPtr.Zero,
                  dwCreationDisposition: NativeMethods.ECreationDisposition.CreateAlways,
                  dwFlagsAndAttributes: NativeMethods.EFileAttributes.Normal, hTemplateFile: IntPtr.Zero
                );

                if (hFile == NativeMethods.INVALID_HANDLE_VALUE)
                {
                    var hr = Marshal.GetHRForLastWin32Error();
                    var ex = Marshal.GetExceptionForHR(hr);
                    throw ex;
                }

                var exceptInfo = new NativeMethods.MINIDUMP_EXCEPTION_INFORMATION();

                var process = Process.GetProcessesByName(processName).FirstOrDefault();

                if (process == null)
                {
                    throw new InvalidOperationException($"Specify a valid process name. {processName} process not found.");
                }

                if (!process.Is32BitProcess() && IntPtr.Size == 4)
                {
                    throw new InvalidOperationException("Can't create 32 bit dump of 64 bit process");
                }

                var isDumpWrittenSuccessfully = NativeMethods.MiniDumpWriteDump(process.Handle, process.Id, hFile,
                          dumpType, ref exceptInfo, UserStreamParam: IntPtr.Zero, CallbackParam: IntPtr.Zero);

                if (!isDumpWrittenSuccessfully)
                {
                    var hr = Marshal.GetHRForLastWin32Error();
                    var ex = Marshal.GetExceptionForHR(hr);
                    throw ex;
                }

                return dumpFileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                NativeMethods.CloseHandle(hFile);
            }
        }
    }
}
