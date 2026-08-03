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
    private BootstrapConfiguration? bootstrapConfiguration;

    public MainWindow()
    {
        InitializeComponent();
        var repositoryRoot = FindRepositoryRoot();
        services = new ObservableCollection<ServiceEntry>(DevelopmentServiceCatalog.Create(repositoryRoot));
        ServicesGrid.ItemsSource = services;
        LoadBootstrapValues();
        UpdateProviderFields();
        TryApplyBootstrap(false);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => RefreshStatus();
        timer.Start();
        RefreshStatus();
    }

    private void Start_Click(object sender, RoutedEventArgs e) => Start((ServiceEntry)((Button)sender).Tag);
    private void Stop_Click(object sender, RoutedEventArgs e) => Stop((ServiceEntry)((Button)sender).Tag);
    private void Restart_Click(object sender, RoutedEventArgs e) { var service = (ServiceEntry)((Button)sender).Tag; Stop(service); Start(service); }
    private void Open_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(((ServiceEntry)((Button)sender).Tag).Url) { UseShellExecute = true });
    private void StartAll_Click(object sender, RoutedEventArgs e) { foreach (var service in services) if (!Start(service)) break; }
    private void StopAll_Click(object sender, RoutedEventArgs e) { foreach (var service in services) Stop(service); }
    private void KeyProvider_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateProviderFields();
    private void GenerateLocalKey_Click(object sender, RoutedEventArgs e)
    {
        LocalConfigurationKey.Password = BootstrapConfiguration.GenerateLocalKey();
        BootstrapStatus.Text = "A new local key was generated. Apply or save it before restarting the API.";
    }
    private void ApplyBootstrap_Click(object sender, RoutedEventArgs e) => TryApplyBootstrap(true);
    private void SaveBootstrap_Click(object sender, RoutedEventArgs e)
    {
        if (!TryApplyBootstrap(false)) return;
        try
        {
            SetEnvironmentVariables(EnvironmentVariableTarget.User, bootstrapConfiguration);
            SetEnvironmentVariables(EnvironmentVariableTarget.Process, bootstrapConfiguration);
            BootstrapStatus.Text = "Bootstrap values were saved to the current Windows user environment. Restart the API to apply them.";
        }
        catch (Exception exception) when (exception is ArgumentException or System.Security.SecurityException)
        {
            BootstrapStatus.Text = "Windows could not save the bootstrap values. No values were written to the repository.";
        }
    }
    private void ClearBootstrap_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, "Clear all saved Vennu configuration bootstrap values for the current Windows user?", "Clear bootstrap values", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            SetEnvironmentVariables(EnvironmentVariableTarget.User, null);
            SetEnvironmentVariables(EnvironmentVariableTarget.Process, null);
            ConfigurationEnvironment.SelectedIndex = 0;
            KeyProvider.SelectedIndex = 0;
            ConfigurationConnectionString.Clear();
            LocalConfigurationKey.Clear();
            KeyVaultKeyId.Clear();
            bootstrapConfiguration = null;
            UpdateProviderFields();
            BootstrapStatus.Text = "Saved bootstrap values were cleared. API startup is blocked until valid values are applied.";
        }
        catch (Exception exception) when (exception is ArgumentException or System.Security.SecurityException)
        {
            BootstrapStatus.Text = "Windows could not clear all saved bootstrap values.";
        }
    }

    private bool Start(ServiceEntry service)
    {
        if (IsListening(service.Port) || ownedProcesses.ContainsKey(service.Name)) return true;
        if (service.Name == "API" && !TryApplyBootstrap(false)) return false;
        var info = new ProcessStartInfo(service.FileName, service.Arguments) { WorkingDirectory = service.WorkingDirectory, UseShellExecute = false };
        foreach (var setting in service.Environment)
        {
            var pair = setting.Split('=', 2);
            info.Environment[pair[0]] = pair[1];
        }
        if (service.Name == "API") bootstrapConfiguration!.ApplyTo(info.Environment);
        var process = Process.Start(info);
        if (process is not null) ownedProcesses[service.Name] = process;
        RefreshStatus();
        return process is not null;
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

    private bool TryApplyBootstrap(bool showSuccess)
    {
        var environmentName = SelectedContent(ConfigurationEnvironment);
        var provider = SelectedContent(KeyProvider);
        if (!BootstrapConfiguration.TryCreate(
            environmentName,
            ConfigurationConnectionString.Password,
            provider,
            LocalConfigurationKey.Password,
            KeyVaultKeyId.Text,
            out var configuration,
            out var error))
        {
            bootstrapConfiguration = null;
            BootstrapStatus.Text = $"API configuration is invalid: {error}";
            return false;
        }
        bootstrapConfiguration = configuration;
        if (showSuccess) BootstrapStatus.Text = "Bootstrap values will be used for API processes started by this control panel. Restart a running API to apply them.";
        return true;
    }

    private void LoadBootstrapValues()
    {
        SelectContent(ConfigurationEnvironment, ReadBootstrapValue(BootstrapConfiguration.EnvironmentVariable) ?? "Development");
        SelectContent(KeyProvider, ReadBootstrapValue(BootstrapConfiguration.KeyProviderVariable) ?? "Environment");
        ConfigurationConnectionString.Password = ReadBootstrapValue(BootstrapConfiguration.ConnectionStringVariable) ?? string.Empty;
        LocalConfigurationKey.Password = ReadBootstrapValue(BootstrapConfiguration.LocalKeyVariable) ?? string.Empty;
        KeyVaultKeyId.Text = ReadBootstrapValue(BootstrapConfiguration.KeyIdVariable) ?? string.Empty;
    }

    private void UpdateProviderFields()
    {
        if (LocalKeyPanel is null || KeyVaultPanel is null) return;
        var useLocalKey = SelectedContent(KeyProvider) == "Environment";
        LocalKeyPanel.IsEnabled = useLocalKey;
        KeyVaultPanel.IsEnabled = !useLocalKey;
    }

    private static string? ReadBootstrapValue(string name) =>
        Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User) ?? Environment.GetEnvironmentVariable(name);

    private static void SetEnvironmentVariables(EnvironmentVariableTarget target, BootstrapConfiguration? configuration)
    {
        foreach (var name in BootstrapConfiguration.VariableNames) Environment.SetEnvironmentVariable(name, null, target);
        if (configuration is null) return;
        foreach (var value in configuration.Values) Environment.SetEnvironmentVariable(value.Key, value.Value, target);
    }

    private static string? SelectedContent(ComboBox comboBox) => (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

    private static void SelectContent(ComboBox comboBox, string value)
    {
        var match = comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault(item => string.Equals(item.Content?.ToString(), value, StringComparison.Ordinal));
        comboBox.SelectedItem = match ?? comboBox.Items[0];
    }

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
