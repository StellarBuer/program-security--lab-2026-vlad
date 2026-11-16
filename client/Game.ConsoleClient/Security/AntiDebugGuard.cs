using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Game.ConsoleClient.Security;

/// <summary>
/// Механізм активної протидії динамічному аналізу та налагодженню (Anti-Debugging).
/// Виявляє підключені відладчики (Visual Studio, WinDbg, dnSpy, x64dbg) та точки зупину.
/// </summary>
public static class AntiDebugGuard
{
    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsDebuggerPresent();

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] ref bool isDebuggerPresent);

    /// <summary>
    /// Перевіряє, чи перебуває процес під контролем налагоджувача.
    /// </summary>
    public static bool DetectDebugger()
    {
        // 1. Стандартна перевірка керованого середовища .NET
        if (Debugger.IsAttached)
        {
            return true;
        }

        // 2. Нативна перевірка Win32 API (PEB.BeingDebugged)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                if (IsDebuggerPresent())
                {
                    return true;
                }

                var isRemote = false;
                if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isRemote) && isRemote)
                {
                    return true;
                }
            }
            catch
            {
                // У разі блокування викликів повертаємо безпечний результат
            }
        }

        // 3. Часовий аналіз (Timing Check): виявлення покрокового виконання (Single-stepping / Breakpoints)
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            _ = i * i;
        }
        sw.Stop();

        // Якщо 1000 простих ітерацій тривали понад 100 мс — це свідчить про точку зупину
        return sw.ElapsedMilliseconds > 100;
    }
}
