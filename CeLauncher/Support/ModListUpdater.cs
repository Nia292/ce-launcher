using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CeLauncher.Support;

public class ModListUpdater
{
    private readonly string[] _modIds;

    public ModListUpdater(string[] modIds)
    {
        _modIds = modIds;
    }

    public async Task Update()
    {
        await DownloadSteamCmdIfNeeded();
        Trace.WriteLine("steamcmd is up to date, updating mods");
        await UpdateMods();
    }
    
    private async Task DownloadSteamCmdIfNeeded()
    {
        if (!File.Exists("./steamcmd.exe"))
        {
            using var client = new HttpClient();
            // Download the Web resource and save it into the current filesystem folder.
            var response = await client.GetByteArrayAsync("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
            await File.WriteAllBytesAsync("./steamcmd.zip", response);
            System.IO.Compression.ZipFile.ExtractToDirectory("./steamcmd.zip", "./");
        }
    }

    private async Task UpdateMods()
    {
        var steamLibraryDir = CeLauncherUtils.FindSteamLibraryWithConan();
        Trace.WriteLine($"Workshop directory is {steamLibraryDir}");
        var updateStatements = _modIds
            .Where(modId => modId != "2886779102")
            .Select(modId => $"+workshop_download_item 440900 {modId} validate");
        var updateStatement = string.Join(" ", updateStatements);
        var command = $"+force_install_dir \"{steamLibraryDir}\" +login anonymous {updateStatement} +quit";
        await CeLauncherUtils.RunProcessAsync("./steamcmd.exe", command);
    }
}