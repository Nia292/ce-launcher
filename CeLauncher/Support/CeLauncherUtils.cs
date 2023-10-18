using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using log4net;
using Microsoft.Win32;

namespace CeLauncher.Support;

public static class CeLauncherUtils
{
    
    private static readonly ILog Log = LogManager.GetLogger(typeof(CeLauncherUtils));
    public static Task<int> RunProcessAsync(string fileName, string args)
    {
        var tcs = new TaskCompletionSource<int>();

        var process = new Process
        {
            StartInfo = { FileName = fileName, Arguments = args },
            EnableRaisingEvents = true
        };

        process.Exited += (sender, args) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();

        return tcs.Task;
    }
    
    public static string FindConanInstallLocation()
    {
        RegistryKey localMachine = Registry.LocalMachine;
        var software = localMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 440900");
        if (software == null)
        {
            Log.Info(@"Failed to find HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 440900");
            throw new Exception(@"Failed to find HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 440900");
        }
        var installLocation = software?.GetValue("InstallLocation");
        if (installLocation == null)
        {
            Log.Info(@"Failed to find HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 440900\InstallLocation");
            throw new Exception(@"Failed to find HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 440900\InstallLocation");
        }

        return (string)installLocation;
    }
    
    public static string FindSteamLibraryWithConan()
    {
        var conanInstallDirectory = FindConanInstallLocation();
        var path = $"{conanInstallDirectory}/../../..";
        return new DirectoryInfo(path).FullName;
    }
}