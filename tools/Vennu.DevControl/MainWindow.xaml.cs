using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Vennu.DevControl;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<ServiceEntry> services;
    private readonly Dictionary<string, Process> ownedProcesses = new(StringComparer.Ordinal);

    public MainWindow()
    {
        InitializeComponent();
        var repositoryRoot = FindRepositoryRoot();
        services =
        [
            new("API", 5192, repositoryRoot, "dotnet", "run --no-build --launch-profile http --project .\\src\\Vennu.Api\\Vennu.Api.csproj", "http://localhost:5192", ["ASPNETCORE_ENVIRONMENT=Development"]),
            new("Admin", 5173, Path.Combine(repositoryRoot, "src", "admin"), "cmd.exe", "/c npm run dev -- --host localhost --port 5173", "http://localhost:5173", ["VITE_VENNU_API_BASE_URL=http://localhost:5192", "VITE_VENNU_DISPLAY_BASE_URL=http://localhost:5175", "VITE_VENNU_VENUE_ADMIN_BASE_URL=http://localhost:5174/venue-admin/"]),
            new("Venue Admin", 5174, Path.Combine(repositoryRoot, "src", "venue-admin"), "cmd.exe", "/c npm run dev -- --host localhost --port 5174", "http://localhost:5174/venue-admin/", ["VITE_VENNU_API_BASE_URL=http://localhost:5192"]),
            new("Display", 5175, Path.Combine(repositoryRoot, "src", "display"), "cmd.exe", "/c npm run dev -- --host localhost --port 5175", "http://localhost:5175", ["VITE_API_BASE_URL=http://localhost:5192", "VITE_SIGNALR_HUB_URL=http://localhost:5192/hubs/vennu"])
        ];
        ServicesGrid.ItemsSource = services;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => RefreshStatus();
        timer.Start();
        RefreshStatus();
    }

    private void Start_Click(object sender, RoutedEventArgs e) => Start((ServiceEntry)((Button)sender).Tag);
    private void Stop_Click(object sender, RoutedEventArgs e) => Stop((ServiceEntry)((Button)sender).Tag);
    private void Restart_Click(object sender, RoutedEventArgs e) { var service = (ServiceEntry)((Button)sender).Tag; Stop(service); Start(service); }
    private void Open_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(((ServiceEntry)((Button)sender).Tag).Url) { UseShellExecute = true });
    private void StartAll_Click(object sender, RoutedEventArgs e) { foreach (var service in services) Start(service); }
    private void StopAll_Click(object sender, RoutedEventArgs e) { foreach (var service in services) Stop(service); }

    private void Start(ServiceEntry service)
    {
        if (IsListening(service.Port) || ownedProcesses.ContainsKey(service.Name)) return;
        var info = new ProcessStartInfo(service.FileName, service.Arguments) { WorkingDirectory = service.WorkingDirectory, UseShellExecute = false };
        foreach (var setting in service.Environment)
        {
            var pair = setting.Split('=', 2);
            info.Environment[pair[0]] = pair[1];
        }
        var process = Process.Start(info);
        if (process is not null) ownedProcesses[service.Name] = process;
        RefreshStatus();
    }

    private void Stop(ServiceEntry service)
    {
        if (!ownedProcesses.Remove(service.Name, out var process) || process.HasExited) return;
        Process.Start(new ProcessStartInfo("taskkill.exe", $"/PID {process.Id} /T /F") { UseShellExecute = false, CreateNoWindow = true })?.WaitForExit();
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        foreach (var service in services) service.Status = IsListening(service.Port) ? "Running" : ownedProcesses.ContainsKey(service.Name) ? "Starting" : "Stopped";
        ServicesGrid.Items.Refresh();
    }

    private static bool IsListening(int port) => IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(endpoint => endpoint.Port == port);

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src")) && Directory.Exists(Path.Combine(directory.FullName, "scripts"))) return directory.FullName;
        }
        throw new InvalidOperationException("Could not locate the Vennu repository root.");
    }
}

public sealed class ServiceEntry(string name, int port, string workingDirectory, string fileName, string arguments, string url, string[] environment)
{
    public string Name { get; } = name;
    public int Port { get; } = port;
    public string WorkingDirectory { get; } = workingDirectory;
    public string FileName { get; } = fileName;
    public string Arguments { get; } = arguments;
    public string Url { get; } = url;
    public string[] Environment { get; } = environment;
    public string Status { get; set; } = "Stopped";
}
