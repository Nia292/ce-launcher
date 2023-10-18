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
        var serverModListExists = File.Exists($"{installLocation}/ConanSandbox/servermodlist.txt");
        if (serverModListExists)
        {
            File.Copy($"{installLocation}/ConanSandbox/servermodlist.txt", $"{installLocation}/ConanSandbox/servermodlist.old.txt", true);
        }

        var modList = _modIds.Select(modId =>
        {
            var modPath = $"{installLocation}\\..\\..\\workshop\\content\\440900\\{modId}";
            var modFile = Directory.GetFiles(modPath).First(file => file.EndsWith(".pak"));
            return new FileInfo(modFile).FullName;
        });
        var modListContent = string.Join("\n", modList);
        File.WriteAllText($"{installLocation}/ConanSandbox/servermodlist.txt", modListContent, Encoding.UTF8);
    }
}