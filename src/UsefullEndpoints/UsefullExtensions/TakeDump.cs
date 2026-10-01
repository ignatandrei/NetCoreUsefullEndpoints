using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace UsefullExtensions;
//from https://devblogs.microsoft.com/dotnet/creating-a-memory-dump-in-csharp/
internal class TakeDump
{
    public static string WriteCurrentProcess(string? path =null)
    {
        //null or whitespace path means use default path in the current directory with a timestamped filename
        if (string.IsNullOrWhiteSpace(path)) path = null;
        path ??= Path.Combine(AppContext.BaseDirectory, $"fulldump-{Environment.ProcessId}-{DateTime.Now:yyyyMMdd-HHmmss}.dmp");


        if (OperatingSystem.IsWindows())
        {
            WindowsDumper.WriteCurrentProcess(path);
        }
        else if (OperatingSystem.IsLinux())
        {
            LinuxDumper.WriteCurrentProcess(path);
        }
        else
        {
            throw new PlatformNotSupportedException("Memory dump is only supported on Windows and Linux.");
        }
        return path;
    }
}
[SupportedOSPlatform("windows")]
internal static class WindowsDumper
{
    [Flags]
    private enum DumpType : uint
    {
        Normal = 0x00000000,
        WithDataSegs = 0x00000001,
        WithFullMemory = 0x00000002,
        WithHandleData = 0x00000004,
        WithUnloadedModules = 0x00000020,
        WithFullMemoryInfo = 0x00000800,
        WithThreadInfo = 0x00001000,
        WithTokenInformation = 0x00040000,
    }

    [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        SafeHandle hFile,
        DumpType dumpType,
        IntPtr exceptionParam,
        IntPtr userStreamParam,
        IntPtr callbackParam);

    /// <summary>
    /// Writes a full memory dump of the current process.
    /// </summary>
    public static void WriteCurrentProcess(string path)
    {
        Write(Process.GetCurrentProcess(), path);
    }

    /// <summary>
    /// Writes a full memory dump of <paramref name="process"/> to <paramref name="path"/>.
    /// </summary>
    public static void Write(Process process, string path)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentException.ThrowIfNullOrEmpty(path);

        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        // Full memory dump: entire address space (including the heap), handles, modules and thread state.
        bool success = MiniDumpWriteDump(
            process.Handle,
            (uint)process.Id,
            stream.SafeFileHandle,
            DumpType.WithFullMemory |
                DumpType.WithFullMemoryInfo |
                DumpType.WithDataSegs |
                DumpType.WithHandleData |
                DumpType.WithUnloadedModules |
                DumpType.WithThreadInfo |
                DumpType.WithTokenInformation,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (!success)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"MiniDumpWriteDump failed for process {process.Id}.");
        }
    }
}

[SupportedOSPlatform("linux")]
internal static class LinuxDumper
{
    // Yama LSM (see /proc/sys/kernel/yama/ptrace_scope). With the default scope of 1
    // ("restricted ptrace"), a process may only be ptraced by its own descendants unless
    // it explicitly designates another process (or PR_SET_PTRACER_ANY) as an allowed
    // tracer via prctl(PR_SET_PTRACER, ...). "Yama" spelled out in ASCII.
    private const int PR_SET_PTRACER = 0x59616d61;
    private static readonly IntPtr PR_SET_PTRACER_ANY = new(-1);

    [DllImport("libc", SetLastError = true)]
    private static extern int prctl(int option, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5);

    public static void WriteCurrentProcess(string path)
    {
        AllowAnyProcessToPtraceSelf();

        Write(Process.GetCurrentProcess(), path);
    }

    /// <summary>
    /// Best-effort: on distros using the Yama LSM (e.g. Ubuntu/Debian) with the default
    /// ptrace_scope of 1 ("restricted ptrace"), a process may only be ptraced by its own
    /// descendants - not the parent that spawned it. createdump attaches to us as our
    /// child, so we explicitly allow any process to ptrace us. This is a no-op (and
    /// harmless) on distros where Yama isn't enabled (e.g. many Fedora/RHEL setups), and
    /// is swallowed entirely if "libc" or prctl can't be resolved at all, which can happen
    /// on musl-based distros like Alpine that don't ship an unversioned libc.so.
    /// </summary>
    private static void AllowAnyProcessToPtraceSelf()
    {
        try
        {
            _ = prctl(PR_SET_PTRACER, PR_SET_PTRACER_ANY, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            // libc/prctl isn't resolvable this way on this platform (e.g. musl/Alpine) -
            // fall through and let createdump itself report any real permission failure.
        }
    }

    public static void Write(Process process, string path)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentException.ThrowIfNullOrEmpty(path);

        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string createDumpPath = FindCreateDump();

        using Process createDump = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = createDumpPath,
                // --full: entire address space (analogous to MiniDumpWithFullMemory).
                // -f: explicit output path (createdump would otherwise pick its own name/location).
                ArgumentList =
                {
                    "--full",
                    "-f", path,
                    process.Id.ToString(),
                },
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        createDump.Start();
        string stdout = createDump.StandardOutput.ReadToEnd();
        string stderr = createDump.StandardError.ReadToEnd();
        createDump.WaitForExit();

        if (createDump.ExitCode != 0)
        {
            string hint = process.Id != Environment.ProcessId
                ? " Dumping another process typically requires running as root, the " +
                  "CAP_SYS_PTRACE capability, or /proc/sys/kernel/yama/ptrace_scope set to 0."
                : " If this is a container, ensure ptrace isn't blocked by seccomp " +
                  "(add --cap-add=SYS_PTRACE) or by an SELinux/AppArmor policy.";

            throw new InvalidOperationException(
                $"createdump failed for process {process.Id} with exit code {createDump.ExitCode}.{hint}{Environment.NewLine}{stdout}{stderr}");
        }
    }

    private static string FindCreateDump()
    {
        string runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        string candidate = Path.Combine(runtimeDirectory, "createdump");

        if (!File.Exists(candidate))
        {
            throw new FileNotFoundException(
                $"Could not find the 'createdump' utility next to the runtime directory '{runtimeDirectory}'.",
                candidate);
        }

        return candidate;
    }
}
