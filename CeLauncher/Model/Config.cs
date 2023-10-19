using System.IO;
using Json.Net;

namespace CeLauncher.Model;

public class Config
{
    public string LastServer;

    public static Config InitFromConfig()
    {
        if (File.Exists("./config.json"))
        {
            var json = File.ReadAllText("./config.json");
            return JsonNet.Deserialize<Config>(json);
        }
        return new Config {LastServer = ""};
    }

    public void Persist()
    {
        File.WriteAllText("./config.json", JsonNet.Serialize(this));
    }
}