using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class NetSilencer
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to NetSilencer!");
        Console.Title = "NetSilencer by BlasterEngine";
        string target = string.Empty;
        int[] portsToScan = GetDefaultPortsToScan(); // Default to common ports
        int timeoutMilliseconds = 5000; // Default timeout

        string logFileName = "scan_log.txt";
        bool logEnabled = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-p" && i + 1 < args.Length && int.TryParse(args[i + 1], out int customPort))
            {
                portsToScan = new[] { customPort };
                i++; // Skip the next argument (the custom port)
            }
            else if (args[i] == "-r" && i + 2 < args.Length && int.TryParse(args[i + 1], out int startPort) && int.TryParse(args[i + 2], out int endPort))
            {
                portsToScan = GetPortRange(startPort, endPort);
                i += 2; // Skip the next two arguments (start and end port)
            }
            else if (args[i] == "-t" && i + 1 < args.Length && int.TryParse(args[i + 1], out int customTimeout))
            {
                timeoutMilliseconds = customTimeout;
                i++; // Skip the next argument (the custom timeout)
            }
            else if (args[i] == "-log")
            {
                logEnabled = true;
            }
            else
            {
                target = args[i];
            }
        }

        if (string.IsNullOrEmpty(target))
        {
            Console.WriteLine("Usage: NetSilencer [-p <port> | -r <startPort> <endPort>] [-t <timeout>] [-log] <target>");
            return;
        }

        Console.WriteLine($"Scanning target: {target}");
        Console.WriteLine($"Timeout set to {timeoutMilliseconds} milliseconds");

        List<Task> scanTasks = new List<Task>();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (int port in portsToScan)
        {
            scanTasks.Add(IsPortOpenAsync(target, port, timeoutMilliseconds));
        }

        await Task.WhenAll(scanTasks);

        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;

        Console.WriteLine($"Scan completed in {elapsedTime.TotalSeconds:F2} seconds");

        if (logEnabled)
        {
            LogScanResults(logFileName, target, portsToScan, timeoutMilliseconds, elapsedTime);
        }
    }

    static int[] GetDefaultPortsToScan()
    {
        // Scan common ports from 1 to 1024
        int[] commonPorts = new int[1024];
        for (int i = 0; i < 1024; i++)
        {
            commonPorts[i] = i + 1;
        }
        return commonPorts;
    }

    static int[] GetPortRange(int startPort, int endPort)
    {
        if (startPort <= endPort)
        {
            int[] portRange = new int[endPort - startPort + 1];
            for (int i = 0; i < portRange.Length; i++)
            {
                portRange[i] = startPort + i;
            }
            return portRange;
        }
        else
        {
            Console.WriteLine("Invalid port range. Start port must be less than or equal to end port.");
            Environment.Exit(1);
            return null;
        }
    }

    static async Task IsPortOpenAsync(string host, int port, int timeoutMilliseconds)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                Console.WriteLine($"Port {port} is open");
            }
        }
        catch (SocketException)
        {
            // Port is closed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while scanning port {port}: {ex.Message}");
        }
    }

    static void LogScanResults(string logFileName, string target, int[] ports, int timeoutMilliseconds, TimeSpan elapsedTime)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFileName, true))
            {
                writer.WriteLine($"Scan results for target: {target}");
                writer.WriteLine($"Timeout set to {timeoutMilliseconds} milliseconds");
                writer.WriteLine($"Scan completed in {elapsedTime.TotalSeconds:F2} seconds");
                writer.WriteLine("Open ports:");

                foreach (int port in ports)
                {
                    try
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            client.Connect(target, port);
                            writer.WriteLine($"Port {port} is open");
                        }
                    }
                    catch (SocketException)
                    {
                        // Port is closed
                    }
                }

                writer.WriteLine();
            }
            Console.WriteLine($"Scan results logged to {logFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while writing to log file: {ex.Message}");
        }
    }
}