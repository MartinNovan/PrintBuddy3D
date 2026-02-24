using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PrintBuddy3D.Enums;
using PrintBuddy3D.Models;

namespace PrintBuddy3D.Services;

public class MarlinPrinterControlService : IPrinterControlService, IDisposable
{
    private SerialPort? _serialPort;
    private readonly PrinterModel _printer;
    private CancellationTokenSource? _cts;
    private readonly List<ConsoleLogItem> _history = new();
    private readonly StringBuilder _readBuffer = new();
    public MarlinPrinterControlService(PrinterModel printer)
    {
        _printer = printer;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        var avaiablePorts = SerialPort.GetPortNames().OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
        if (string.IsNullOrEmpty(_printer.LastSerialPort) || !avaiablePorts.Contains(_printer.LastSerialPort)) return;
        _serialPort = new SerialPort(_printer.LastSerialPort, _printer.BaudRate, Parity.None, 8, StopBits.One)
        {
            DtrEnable = false,
            RtsEnable = false,
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 2000,
            NewLine = "\n",
            Encoding = Encoding.ASCII
        };

        try
        {
            _serialPort.Open();
            _cts = new CancellationTokenSource();
            Task.Run(() => ReaderLoop(_cts.Token));
        }
        catch (Exception ex)
        {
            LogToConsole($"Error with connecting to port: {ex.Message}", ConsoleLogType.Error);
        }
    }

    public void Dispose()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        if (_serialPort is { IsOpen: true })
        {
            _serialPort.Close();
        }
        _serialPort?.Dispose();
    }
    private async Task ReaderLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _serialPort is { IsOpen: true })
        {
            try
            {
                int b = _serialPort.BaseStream.ReadByte();
                if (b < 0)
                {
                    await Task.Delay(10, token);
                    continue;
                }

                char ch = (char)b;
                if (ch == '\r') continue;
                if (ch == '\n')
                {
                    string line = _readBuffer.ToString();
                    _readBuffer.Clear();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        LogToConsole(line, ConsoleLogType.Info);
                    }
                }
                else _readBuffer.Append(ch);
            }
            catch
            {
                // ignored
            }
        }
    }

    public void SendCommand(string command)
    {
        if (_serialPort is { IsOpen: true })
        {
            _serialPort.Write(command.TrimEnd() + "\n");
            LogToConsole(command, ConsoleLogType.Command);
        }
    }
    public void EmergencyStop()
    {
        SendCommand("M112"); 
        // Reset board if the board supports it
        _ = PulseDtr();
    }

    private async Task PulseDtr()
    {
        if (_serialPort == null) return;
        var prev = _serialPort.DtrEnable;
        _serialPort.DtrEnable = false;
        await Task.Delay(100);
        _serialPort.DtrEnable = true;
        await Task.Delay(100);
        _serialPort.DtrEnable = prev;
    }

    private void LogToConsole(string message, ConsoleLogType type)
    {
        var item = new ConsoleLogItem(message, DateTime.Now.TimeOfDay.TotalSeconds, type);
        lock (_history) { _history.Add(item); }
    }

    public void Move(string axis, double distance, int speed)
    {
        // G91 = Relative position (move by 10mm, not move to 10mm)
        // G1 {axis}{distance} F{speed} = the travel
        // G90 = Back to the absolute position
        var gcode = $"G91\nG1 {axis}{distance} F{speed}\nG90";
        SendCommand(gcode);
    }

    public void Home(string axis)
    {
        SendCommand(axis.ToLower() == "all" ? "G28" : $"G28 {axis}");
    }

    public void SetTemperature(int temp, string type = "extruder")
    {
        var cmd = type == "heater_bed" ? $"M140 S{temp}" : $"M104 S{temp}"; // Set bed and extruder temp
        SendCommand(cmd);
    }

    public void DisableMotors()
    {
        SendCommand("M84");
    }

    public Task<List<ConsoleLogItem>> GetConsoleHistoryAsync()
    {
        lock (_history)
        {
            return Task.FromResult(_history);
        }
    }
} 