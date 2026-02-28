using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet;

namespace PrintBuddy3D.ViewModels;

public partial class SshWindowViewModel : ObservableObject, IDisposable
{
    private SshClient? _client;
    private ShellStream? _shellStream;
    private CancellationTokenSource? _cts;

    private TaskCompletionSource<string>? _passwordTcs;
    private bool _waitingForPassword;
    private bool _isConnected;

    [ObservableProperty] private string _windowTitle = "SSH";
    [ObservableProperty] private string _output = string.Empty;
    [ObservableProperty] private string _input = string.Empty;
    [ObservableProperty] private bool _isPasswordMode;
    public char PasswordChar => IsPasswordMode ? '*' : '\0';
    
    private readonly string _host;
    private readonly string _username;
    private readonly int _port;
    private static readonly System.Text.RegularExpressions.Regex AnsiRegex = new(@"\x1B(\[[0-9;?]*[a-zA-Z]|\][^\x07]*\x07|[()][0-9A-Za-z]|[^[])", System.Text.RegularExpressions.RegexOptions.Compiled);
    
    public SshWindowViewModel(string host, string username, int port = 22)
    {
        _host = host;
        _username = username;
        _port = port;
        _windowTitle = $"SSH → {host}";

        Dispatcher.UIThread.Post(async () => await ConnectAsync(), DispatcherPriority.Loaded);
    }

    private static string CleanOutput(string text)
    {
        text = AnsiRegex.Replace(text, "");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[\x00-\x09\x0B\x0C\x0E-\x1F\x7F]", "");
        return text;
    }
    private async Task ConnectAsync()
    {
        try
        {
            var password = await WaitForPasswordAsync();

            AppendOutput("Connecting...\n");
            _client = new SshClient(_host, _port, _username, password);

            await Task.Run(() => _client.Connect());

            _shellStream = _client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
            _isConnected = true;

            _cts = new CancellationTokenSource();
            _ = ReadOutputLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            AppendOutput($"{ex.Message}\n");
            _isConnected = false;
            // Probably bad password
            await ConnectAsync();
        }
    }

    private Task<string> WaitForPasswordAsync()
    {
        _passwordTcs = new TaskCompletionSource<string>();
        _waitingForPassword = true;
        IsPasswordMode = true;
        OnPropertyChanged(nameof(PasswordChar));

        AppendOutput($"{_username}@{_host}'s password: ");

        return _passwordTcs.Task;
    }

    [RelayCommand]
    private void SendCommand()
    {
        try
        {
            if (string.IsNullOrEmpty(Input)) return;

            if (_waitingForPassword && _passwordTcs != null)
            {
                var password = Input;
                Input = string.Empty;
                IsPasswordMode = false;
                OnPropertyChanged(nameof(PasswordChar));
                _waitingForPassword = false;
                AppendOutput("\n");

                _passwordTcs.SetResult(password);
                return;
            }

            if (_shellStream == null) return;
            _shellStream.Write(Input + "\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending command: " + ex.Message);
            AppendOutput("Error sending command! This probably means the connection has ended.");
        }
        finally
        {
            Input = string.Empty;
        }
        
    }

    private async Task ReadOutputLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[4096];
        while (!ct.IsCancellationRequested && _shellStream != null)
        {
            try
            {
                await Task.Delay(50, ct);
                if (_shellStream.DataAvailable)
                {
                    int count = _shellStream.Read(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        var clean = CleanOutput(Encoding.UTF8.GetString(buffer, 0, count));
                        AppendOutput(clean);
                    }
                }
            }
            catch (OperationCanceledException) { break; }
        }
    }

    private void AppendOutput(string text)
    {
        Dispatcher.UIThread.Post(() => Output += text);
    }

    public void Dispose()
    {
        _passwordTcs?.TrySetCanceled();
        _cts?.Cancel();
        _shellStream?.Close();
        _client?.Disconnect();
        _client?.Dispose();
        _shellStream?.Dispose();
        _cts?.Dispose();
    }
}