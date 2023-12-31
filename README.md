![launcher.png](launcher.png)
# Conan Exiles Launcher
Just a launcher for Conan Exiles that patches your modlist for you. Has the feature to connect to a server and, optionally, update all your mods before it does. 

Features Include:
- Connecting to a public server from the [ce-servers-list](https://github.com/Nia292/ce-server-list/blob/main/servers.json)
- Connecting to a private server via supplied config 
- Updating your modlist without the steam validation/re-sub hassle

# Requirements
Currently only works on 64bit Windows. If you need a 32bit version let me know. Not sure if I can support Linux natively.

# Installation
Head over to the releases tab. Download the latest version.

You may be prompted to download and install a .NET desktop runtime. Follow the steps instructed when it happens. 

# Updating Mods
You can force-update the mods for the server you have currently selected by hitting "Update and Launch". This will download steamcmd for you and then update each mod in the servers modlist.
**Sometimes, on the first run,  it will hang on the first mod in the modlist for no reason whatsoever. If you see your download being stuck in the console window that pops up, close it and hit Update and Launch again.**

# My server isn't listed
The launcher pulls servers from here: https://github.com/Nia292/ce-server-list/blob/main/servers.json

Your server admin needs to get in touch with me, ideally via the TW mod discord or directly (@nia2424), then I can add the server there. 

# Supplying additional servers
Should you, for any reason, not wish to list your servers in the public server list you can supply a ``servers.json`` config-file alongside the launcher. These servers will, additionally, be loaded into the launcher.

The servers.json shoudl look like this:
```json
[
	{
	  "Name": "My Server",
	  "Host": "127.0.0.1",
	  "QueryPort": "27015",
	  "HasPassword": false,
	  "Mods": ["880454836","3036057084","2846119484","2847709656","1823412793","2377569193","2992829097","1701455174","1966733568","877108545","2250037083","933782986","2752945598","1928978003","1855055876","2411388528","2723987721","2644416526","2050780234","2376449518","1326031593","1797359985","2886779102","2300463941","3036058836","1369743238","2001044383","1923957401","2305969565"]
	}
]
```
Where you obviously have to set name, host, query port and the modlist. If you have issues with this, feel free to reach out.

# Future Plans

Right now, this is just a drop-in replacement for the funcom launcher. It only provides marginally more features, like server selection and
hassle-free modlist management.

I initially intended this to be a replacement for steam workshop, providing servers with a means to directly supply mods to their
userbase through nexusmods/gdrive/etc. Right now, I'd like to test this a bit further, see how it works and if there is interest!

