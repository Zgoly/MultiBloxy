using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MultiBloxy
{
    public class HandleCloser
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_HANDLE_INFORMATION
        {
            public uint ProcessId;
            public byte ObjectTypeNumber;
            public byte Flags;
            public ushort Handle;
            public uint Object;
            public uint GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_NAME_INFORMATION
        {
            public UNICODE_STRING Name;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenEvent(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("ntdll.dll")]
        private static extern uint NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, uint systemInformationLength, out uint returnLength);

        [DllImport("ntdll.dll")]
        private static extern uint NtQueryObject(IntPtr handle, int objectInformationClass, IntPtr objectInformation, uint objectInformationLength, out uint returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        private const int SystemHandleInformation = 16;
        private const int ObjectNameInformation = 1;
        private const uint PROCESS_ALL = 0x001F0FFF;
        private const uint DUPLICATE_CLOSE_SOURCE = 0x0001;
        private const uint DUPLICATE_SAME_ACCESS = 0x0002;

        // I don't think this is the most ideal and optimized solution. Feel free to create a pull request with improvements / refactoring
        public static void CloseAllHandles()
        {
            // Get all processes with the name "RobloxPlayerBeta"
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            // Iterate through each process
            foreach (var process in processes)
            {
                uint size = 0x10000;
                IntPtr buffer = Marshal.AllocHGlobal((int)size);

                // Loop to query system information until the buffer is large enough
                while (true)
                {
                    // Query system information for handles
                    uint status = NtQuerySystemInformation(SystemHandleInformation, buffer, size, out uint _);

                    // If the buffer is too small, double its size and retry
                    if (status == 0xC0000004)
                    {
                        size *= 2;
                        Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal((int)size);
                    }
                    else
                    {
                        break;
                    }
                }

                // Read the number of handles from the buffer
                int handleCount = Marshal.ReadInt32(buffer);
                IntPtr ptr = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));

                // Iterate through each handle
                for (int i = 0; i < handleCount; i++)
                {
                    // Get handle information from the buffer
                    SYSTEM_HANDLE_INFORMATION handleInfo = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ptr, typeof(SYSTEM_HANDLE_INFORMATION));

                    // Check if the handle belongs to the current process
                    if (handleInfo.ProcessId == process.Id)
                    {
                        // Open the process to get a handle
                        IntPtr processHandle = OpenProcess(PROCESS_ALL, false, process.Id);
                        if (processHandle == IntPtr.Zero)
                        {
                            // Move to the next handle if opening the process fails
                            ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION)));
                            continue;
                        }

                        // Duplicate the handle to the current process
                        bool success = DuplicateHandle(processHandle, new IntPtr(handleInfo.Handle), GetCurrentProcess(), out IntPtr dupHandle, 0, false, DUPLICATE_SAME_ACCESS);
                        if (!success)
                        {
                            // Close the process handle and move to the next handle if duplication fails
                            CloseHandle(processHandle);
                            ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION)));
                            continue;
                        }

                        uint bufferSize = 0x1000;
                        IntPtr nameBuffer = Marshal.AllocHGlobal((int)bufferSize);

                        // Query the object name information for the duplicated handle
                        uint status = NtQueryObject(dupHandle, ObjectNameInformation, nameBuffer, bufferSize, out uint _);

                        if (status != 0)
                        {
                            // Free resources and move to the next handle if querying the object name fails
                            Marshal.FreeHGlobal(nameBuffer);
                            CloseHandle(dupHandle);
                            CloseHandle(processHandle);
                            ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION)));
                            continue;
                        }

                        // Get the object name information from the buffer
                        OBJECT_NAME_INFORMATION objectNameInfo = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(nameBuffer, typeof(OBJECT_NAME_INFORMATION));
                        if (objectNameInfo.Name.Length > 0)
                        {
                            // Convert the object name to a string
                            string name = Marshal.PtrToStringUni(objectNameInfo.Name.Buffer, objectNameInfo.Name.Length / 2);
                            if (name.Contains("ROBLOX_singletonEvent"))
                            {
                                // Close the handle if it matches the target name
                                DuplicateHandle(processHandle, new IntPtr(handleInfo.Handle), IntPtr.Zero, out _, 0, false, DUPLICATE_CLOSE_SOURCE);
                            }
                        }

                        // Free resources
                        Marshal.FreeHGlobal(nameBuffer);
                        CloseHandle(dupHandle);
                        CloseHandle(processHandle);
                    }
                    // Move to the next handle
                    ptr = new IntPtr(ptr.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION)));
                }

                // Free the allocated buffer
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}