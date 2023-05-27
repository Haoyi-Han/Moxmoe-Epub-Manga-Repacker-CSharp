using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MoxmoeApp.MoeUtils;

public class TaskbarIndicator
{
    private readonly IntPtr _handle;
    private readonly ITaskbarList3 _taskbarList;
    private readonly bool _taskbarSupported;

    public TaskbarIndicator()
    {
        _handle = Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero
            ? Process.GetCurrentProcess().MainWindowHandle
            : GetConsoleWindow();
        _taskbarList = (ITaskbarList3)new TaskbarInstance();
        _taskbarSupported = Environment.OSVersion.Version >= new Version(6, 1);
    }

    public void SetProgressValue(int currVal, int totVal)
    {
        if (_taskbarSupported) _taskbarList.SetProgressValue(_handle, (ulong)currVal, (ulong)totVal);
    }

    public void ResetProgressState()
    {
        if (_taskbarSupported) _taskbarList.SetProgressState(_handle, TaskbarStates.NoProgress);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    private static extern bool FreeConsole();

    private enum TaskbarStates
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    [ComImport]
    [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        [PreserveSig]
        void HrInit();

        [PreserveSig]
        void AddTab(IntPtr hwnd);

        [PreserveSig]
        void DeleteTab(IntPtr hwnd);

        [PreserveSig]
        void ActivateTab(IntPtr hwnd);

        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

        [PreserveSig]
        void SetProgressState(IntPtr hwnd, TaskbarStates state);
    }

    [ComImport]
    [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
    [ClassInterface(ClassInterfaceType.None)]
    private class TaskbarInstance
    {
    }
}