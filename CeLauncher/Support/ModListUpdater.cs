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
        var steamLibraryDir = CeLauncherUtils.FindSteamLibraryWithConan();
        Trace.WriteLine($"Workshop directory is {steamLibraryDir}");
        var updateStatements = _modIds
            .Where(modId => modId != "2886779102")
            .Select(modId => $"+workshop_download_item 440900 {modId} validate");
        var updateStatement = string.Join(" ", updateStatements);
        var command = $"+force_install_dir \"{steamLibraryDir}\" +login anonymous {updateStatement} +quit";
        var result = await CeLauncherUtils.RunProcessAsync($"{SteamCmdDirPath()}\\steamcmd.exe", command);
        if (result != 0)
        {
            Log.Info("Failed to update: " + result);
            return false;
        }

        return true;
    }

    private string SteamCmdDirPath()
    {
        return "./steam-cmd";
    }
}