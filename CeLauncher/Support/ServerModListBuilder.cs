using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CeLauncher.Support;

public class ServerModListBuilder
{
    private readonly string[] _modIds;

    public ServerModListBuilder(string[] modIds)
    {
        _modIds = modIds;
    }

    public void BuildServerModList()
    {
        var installLocation = CeLauncherUtils.FindConanInstallLocation();
        var serverModListExists = File.Exists($"{installLocation}/ConanSandbox/Mods/modlist.txt");
        if (serverModListExists)
        {
            var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();
            File.Copy($"{installLocation}/ConanSandbox/servermodlist.txt", $"{installLocation}/ConanSandbox/Mods/modlist.backup.{ts}.txt", true);
        }

        var modList = _modIds.Select(modId =>
        {
            var modPath = $"{installLocation}\\..\\..\\workshop\\content\\440900\\{modId}";
            var modFile = Directory.GetFiles(modPath).First(file => file.EndsWith(".pak"));
            return new FileInfo(modFile).FullName;
        });
        var modListContent = string.Join("\n", modList);
        File.WriteAllText($"{installLocation}/ConanSandbox/Mods/modlist.txt", modListContent, Encoding.UTF8);
    }

    public bool AnyModsMissing()
    {
        var installLocation = CeLauncherUtils.FindConanInstallLocation();
        return !_modIds.All(modId =>
        {
            var modPath = $"{installLocation}\\..\\..\\workshop\\content\\440900\\{modId}";
            if (!Directory.Exists(modPath))
            {
                return false;
            }
            var numberOfModFiles = Directory.GetFiles(modPath).Where(file => file.EndsWith(".pak"));
            return numberOfModFiles.Any();
        });
    }
}