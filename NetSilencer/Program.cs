using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
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

        string discordWebhookUrl = string.Empty;

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
            else if (args[i] == "-w" && i + 1 < args.Length)
            {
                discordWebhookUrl = args[i + 1];
                i++; // Skip the next argument (the webhook URL)
            }
            else
            {
                target = args[i];
            }
        }

        if (string.IsNullOrEmpty(target))
        {
            Console.WriteLine("Usage: NetSilencer [-p <port> | -r <startPort> <endPort>] [-t <timeout>] [-log] [-w <webhook_url>] <target>");
            return;
        }

        Console.WriteLine($"Scanning target: {target}");
        Console.WriteLine($"Timeout set to {timeoutMilliseconds} milliseconds");

        List<Task<PortScanResult>> scanTasks = new List<Task<PortScanResult>>();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        foreach (int port in portsToScan)
        {
            scanTasks.Add(Task.Run(async () =>
            {
                return await ScanPortAsync(target, port, timeoutMilliseconds);
            }));
        }

        List<int> openPorts = new List<int>();

        // Display the loading bar while waiting for tasks to complete
        while (!Task.WhenAll(scanTasks).IsCompleted)
        {
            int completedTasks = scanTasks.Count(task => task.IsCompleted);
            DisplayLoadingBar(completedTasks, portsToScan.Length);
            await Task.Delay(100);
        }
        Console.Clear(); // Clear the loading bar
        Console.WriteLine($"Scanning ports... {scanTasks.Count}/{portsToScan.Length}");

        foreach (var scanResult in scanTasks)
        {
            if (scanResult.Result.IsOpen)
            {
                Console.WriteLine($"[+] Port {scanResult.Result.Port} is open");

                if (!string.IsNullOrEmpty(scanResult.Result.ServiceBanner))
                {
                    Console.WriteLine($"    Service on port {scanResult.Result.Port}: {scanResult.Result.ServiceBanner}");
                }

                openPorts.Add(scanResult.Result.Port);
            }
        }

        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;

        Console.WriteLine();
        Console.WriteLine($"Scan completed in {elapsedTime.TotalSeconds:F2} seconds");

        if (logEnabled)
        {
            LogScanResults(logFileName, target, openPorts, timeoutMilliseconds, elapsedTime);
        }

        if (!string.IsNullOrEmpty(discordWebhookUrl))
        {
            await SendToDiscordWebhookAsync(discordWebhookUrl, target, openPorts, elapsedTime);
        }
    }

    static void DisplayLoadingBar(int current, int total)
    {
        const int barLength = 40; // The length of the loading bar
        int progress = (int)((double)current / total * barLength);

        Console.Write("[");
        for (int i = 0; i < barLength; i++)
        {
            if (i < progress)
                Console.Write("#");
            else
                Console.Write(" ");
        }
        Console.Write($"] {progress * (100 / barLength)}%\r");
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

    static async Task<PortScanResult> ScanPortAsync(string host, int port, int timeoutMilliseconds)
    {
        PortScanResult result = new PortScanResult { Port = port };

        try
        {
            using (var client = new TcpClient())
            {
                var task = client.ConnectAsync(host, port);
                if (await Task.WhenAny(task, Task.Delay(timeoutMilliseconds)) == task)
                {
                    result.IsOpen = client.Connected;
                    if (client.Connected)
                    {
                        result.ServiceBanner = await GetServiceBannerAsync(client);
                    }
                }
            }
        }
        catch (Exception)
        {
            // Handle any exceptions here
        }

        return result;
    }

    static async Task<string> GetServiceBannerAsync(TcpClient client)
    {
        try
        {
            using (var stream = client.GetStream())
            {
                byte[] banner = new byte[1024];
                int bytesRead = await stream.ReadAsync(banner, 0, banner.Length);
                return Encoding.ASCII.GetString(banner, 0, bytesRead);
            }
        }
        catch (Exception)
        {
            // Handle any exceptions here
        }

        return null;
    }

    static int[] GetPortRange(int startPort, int endPort)
    {
        if (startPort < 1 || startPort > 65535 || endPort < 1 || endPort > 65535 || startPort > endPort)
        {
            throw new ArgumentException("Invalid port range.");
        }

        int[] ports = new int[endPort - startPort + 1];
        for (int i = 0; i < ports.Length; i++)
        {
            ports[i] = startPort + i;
        }

        return ports;
    }

    static void LogScanResults(string logFileName, string target, List<int> openPorts, int timeoutMilliseconds, TimeSpan elapsedTime)
    {
        try
        {
            using (var file = System.IO.File.CreateText(logFileName))
            {
                file.WriteLine($"Scan results for target: {target}");
                file.WriteLine($"Timeout: {timeoutMilliseconds} milliseconds");
                file.WriteLine($"Elapsed Time: {elapsedTime.TotalSeconds:F2} seconds");
                file.WriteLine("Open ports:");

                foreach (int port in openPorts)
                {
                    file.WriteLine($"- Port {port} is open");
                }
            }

            Console.WriteLine($"Scan results saved to {logFileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while writing to log file: {ex.Message}");
        }
    }

    static async Task SendToDiscordWebhookAsync(string webhookUrl, string target, List<int> openPorts, TimeSpan elapsedTime)
    {
        try
        {
            // Create a JSON payload for the Discord message
            var discordMessage = new
            {
                content = $"Scan results for target: {target} \nElapsed Time {elapsedTime.TotalSeconds:F2} seconds \nOpen Ports: {string.Join(", ", openPorts)}"
            };

            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(discordMessage);

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(webhookUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Results sent to Discord successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to send results to Discord. Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending results to Discord: {ex.Message}");
        }
    }
}

class PortScanResult
{
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string ServiceBanner { get; set; }
}
