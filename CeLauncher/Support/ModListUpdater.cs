using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;

namespace CeLauncher.Support;

public class ModListUpdater
{
    private readonly string[] _modIds;
    private static readonly ILog Log = LogManager.GetLogger(typeof(ModListUpdater));

    public ModListUpdater(string[] modIds)
    {
        _modIds = modIds;
        if (!Directory.Exists(SteamCmdDirPath()))
        {
            Directory.CreateDirectory(SteamCmdDirPath());
        }
    }

    public async Task<bool> Update()
    {
        await DownloadSteamCmdIfNeeded();
        Trace.WriteLine("steamcmd is up to date, updating mods");
        return await UpdateMods();
    }
    
    private async Task DownloadSteamCmdIfNeeded()
    {
        if (!File.Exists($"{SteamCmdDirPath()}/steamcmd.exe"))
        {
            using var client = new HttpClient();
            // Download the Web resource and save it into the current filesystem folder.
            var response = await client.GetByteArrayAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
            await File.WriteAllBytesAsync($"{SteamCmdDirPath()}\\steamcmd.zip", response);
            System.IO.Compression.ZipFile.ExtractToDirectory($"{SteamCmdDirPath()}\\steamcmd.zip", SteamCmdDirPath());
        }
    }

    private async Task<bool> UpdateMods()
    {
        if (_modIds.Length <= 0)
        {
            return true;
        }
        var steamLibraryDir = CeLauncherUtils.FindSteamLibraryWithConan();
        Trace.WriteLine($"Workshop directory is {steamLibraryDir}");
        var resultFirst = await UpdateFirstMod(steamLibraryDir);
        if (resultFirst != 0)
        {
            Log.Info("Failed to update: " + resultFirst);
            return false;
        }
        var resultBatch = await UpdateModlist(steamLibraryDir);
        if (resultBatch != 0)
        {
            Log.Info("Failed to update: " + resultBatch);
            return false;
        }

        return true;
    }

    /// <summary>
    /// SteanCMD seems to randomly stall when doing a batch up. To bypass that, we do a little warmup round with just the first mod
    /// to then update all the mods after
    /// </summary>
    /// <param name="steamLibraryDir"></param>
    private async Task<int> UpdateFirstMod(string steamLibraryDir)
    {
        var commandFirst = $"+force_install_dir \"{steamLibraryDir}\" +login anonymous +workshop_download_item 440900 {_modIds[0]} validate +quit";
        return await CeLauncherUtils.RunProcessAsync($"{SteamCmdDirPath()}\\steamcmd.exe", commandFirst);
    }
    
    /// <summary>
    /// SteanCMD seems to randomly stall when doing a batch up. To bypass that, we do a little warmup round with just the first mod
    /// to then update all the mods after
    /// </summary>
    /// <param name="steamLibraryDir"></param>
    private async Task<int> UpdateModlist(string steamLibraryDir)
    {
        var updateStatements = BuildUpdateStatements();
        var command = $"+force_install_dir \"{steamLibraryDir}\" +login anonymous {updateStatements} +quit";
        return await CeLauncherUtils.RunProcessAsync($"{SteamCmdDirPath()}\\steamcmd.exe", command);
    }

    
    private string BuildUpdateStatements()
    {
        var statements = _modIds
            // Bypass tot! custom for now
            .Where(modId => modId != "2886779102")
            .Select(modId => $"+workshop_download_item 440900 {modId} validate");
        return string.Join(" ", statements);
    }

    private string SteamCmdDirPath()
    {
        return "./steam-cmd";
    }
}