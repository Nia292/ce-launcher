using System.Collections.Generic;
using System.IO;
using Json.Net;

namespace CeLauncher.Model;

public class ConnectableServer
{
    public string Name;
    public string Host;
    public string QueryPort;
    public bool HasPassword;
    public string[] Mods;

}