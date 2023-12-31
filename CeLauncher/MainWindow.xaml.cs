﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CeLauncher.Model;
using CeLauncher.Support;
using Json.Net;
using log4net;
using Microsoft.Win32;

namespace CeLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConnectableServer? _server;
        private ConnectableServer[] _availableServers = {};
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
        private readonly Config _config;
        
        public MainWindow()
        {
            _config = Config.InitFromConfig();
            InitializeComponent();
            LoadServerList();
        }

        private void BtnLaunch_OnClick(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private RegistryKey GetSteamRegistryKey()
        {
            RegistryKey localMachine = Registry.CurrentUser;
            var steam = localMachine.OpenSubKey(@"SOFTWARE\Valve\Steam");
            if (steam == null)
            {
                throw new Exception(@"Unable to find SOFTWARE\Valve\Steam");
            }

            return steam;
        }

        private string FindSteamExecutable()
        {
            var steamExe = GetSteamRegistryKey().GetValue("SteamExe");
            if (steamExe == null)
            {
                Log.Error("Failed to find SteamExe registry key to resolve steam.exe");
                throw new Exception("Failed to find SteamExe registry key to resolve steam.exe");
            }

            return (string)steamExe;
        }

        private void Window_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnUpdateLaunch_OnClick(object sender, RoutedEventArgs e)
        {
            Log.Info("Updating and Launching");
            var button = (Button)sender;
            button.IsEnabled = false;
            button.Content = "Updating...";
            var result = await new ModListUpdater(_server.Mods).Update();
            if (result)
            {
                Connect();
            }
            else
            {
                MessageBox.Show("SteamCMD failed to update the modlist. Please run again.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
            }
            button.IsEnabled = true;
            button.Content = "Update and Launch";
        }

        private void Connect()
        {
            if (_server != null)
            {
                var modListBuilder = new ServerModListBuilder(_server.Mods);
                Log.Info("connecting to server " + _server.Name);
                var steamExe = FindSteamExecutable();
                Log.Info("Found steam " + steamExe);
                if (modListBuilder.AnyModsMissing())
                {
                    MessageBox.Show("A mod from your servers modlist is missing. Please launch via Update and Launch", "Launch Failed", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
                }
                else
                {
                    modListBuilder.BuildServerModList();
                    Log.Info("Built servermodlist.txt");
                    if (_server.HasPassword)
                    {
                        var password = PasswordBox.Password;
                        Process.Start(steamExe,
                            $"-applaunch 440900 +connect {_server.Host}:{_server.QueryPort} +password {password}"); 
                    }
                    else
                    {
                        Process.Start(steamExe,
                            $"-applaunch 440900 +connect {_server.Host}:{_server.QueryPort}");
                    }
                    Log.Info("Launched Game!");
                }
            }
        }


        private async Task LoadServerList()
        {
            Log.Info("Loading server list");
            SetLoadingServers(true);
            if (File.Exists("./servers.json"))
            {
                _availableServers = _availableServers.Concat(LoadServerListFromFile()).ToArray();
            }
            var serversFromGithub = await LoadServerListFromGithub();
            _availableServers = _availableServers.Concat(serversFromGithub).ToArray();
            var selectedServer = _availableServers.FirstOrDefault(server => _config.LastServer == server.Name, _availableServers[0]);
            SetSelectedServer(selectedServer);
            RebuildServerList();
            SetLoadingServers(false);
            Log.Info("Loaded server list");
        }

        private async Task<ConnectableServer[]> LoadServerListFromGithub()
        {
           
            using var client = new HttpClient();
            var response = await client.GetStringAsync("https://raw.githubusercontent.com/Nia292/ce-server-list/main/servers.json");
            return JsonNet.Deserialize<ConnectableServer[]>(response);
        }
        
        private ConnectableServer[] LoadServerListFromFile()
        {
            var json = File.ReadAllText("./servers.json");
            return JsonNet.Deserialize<ConnectableServer[]>(json);
        }

        private void SetSelectedServer(ConnectableServer server)
        {
            _server = server;
            BtnLaunch.IsEnabled = true;
            BtnUpdateLaunch.IsEnabled = true;
            BtnLaunch.Content = "Launch " + _server.Name;
            CbxServerList.SelectedItem = _server.Name;
            if (server.HasPassword)
            {
                LblServerPassword.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Password = "";
            }
            else
            {
                LblServerPassword.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;
                PasswordBox.Password = "";
            }

            _config.LastServer = server.Name;
            _config.Persist();
        }

        private void SetLoadingServers(bool loading)
        {
            if (loading)
            {
                BtnLaunch.IsEnabled = false;
                BtnUpdateLaunch.IsEnabled = false;
                LblTitle.Content = "Loading Servers...";
            }
            else
            {
                BtnLaunch.IsEnabled = true;
                BtnUpdateLaunch.IsEnabled = true;
                LblTitle.Content = "Conan Exiles Launcher";
            }
        }

        private void CbxServerList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems[0];
            if (selectedItem is string)
            {
                var server = _availableServers.First(server => server.Name == (string)selectedItem);
                SetSelectedServer(server);
            }
        }

        private void RebuildServerList()
        {
            CbxServerList.Items.Clear();
            foreach (var connectableServer in _availableServers)
            {
                CbxServerList.Items.Add(connectableServer.Name);
            }
        }
    }
}