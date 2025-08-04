using System;
using System.Runtime.InteropServices;

namespace Win32 {

  internal static class Kernel32 {

    private const string Dll = "kernel32.dll";

    [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

    [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport(Dll, SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpFileMappingAttributes, uint flProtect, uint dbMaximumSizeHigh, uint dbMaximumSizeLow, string lpName);

    [DllImport(Dll, SetLastError = true)]
    internal static extern bool SetEvent(IntPtr hEvent);

    [DllImport(Dll, SetLastError = true)]
    internal static extern IntPtr MapViewOfFile(IntPtr hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport(Dll, SetLastError = true)]
    internal static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport(Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CloseHandle(IntPtr hObject);

    [DllImport(Dll, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

  }

}
