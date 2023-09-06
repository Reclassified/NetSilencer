using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

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

        bool sendToDiscord = false;
        string discordWebhookUrl = "YOUR_DISCORD_WEBHOOK_URL"; // Replace with your Discord webhook URL

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
                sendToDiscord = true;
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

        List<Task<bool>> scanTasks = new List<Task<bool>>();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        List<int> openPorts = new List<int>();

        foreach (int port in portsToScan)
        {
            scanTasks.Add(Task.Run(async () => await IsPortOpenAsync(target, port, timeoutMilliseconds)));
        }

        await Task.WhenAll(scanTasks);

        for (int i = 0; i < scanTasks.Count; i++)
        {
            int port = portsToScan[i];
            if (scanTasks[i].Result)
            {
                Console.WriteLine($"Port {port} is open");
                openPorts.Add(port);
            }

            DisplayLoadingBar(i + 1, scanTasks.Count);
        }

        Console.WriteLine(); // Move to a new line after the loading bar
        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;

        Console.WriteLine($"Scan completed in {elapsedTime.TotalSeconds:F2} seconds");

        if (logEnabled)
        {
            LogScanResults(logFileName, target, openPorts, timeoutMilliseconds, elapsedTime);
        }

        if (sendToDiscord)
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

    static async Task<bool> IsPortOpenAsync(string host, int port, int timeoutMilliseconds)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(host, port);
                return true;
            }
        }
        catch (SocketException)
        {
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while scanning port {port}: {ex.Message}");
            return false;
        }
    }

    static async Task SendToDiscordWebhookAsync(string webhookUrl, string target, List<int> openPorts, TimeSpan elapsedTime)
    {
        using (HttpClient client = new HttpClient())
        {
            var payload = new
            {
                content = $"Scan results for target: {target}\nElapsed Time: {elapsedTime.TotalSeconds:F2} seconds\nOpen ports: {string.Join(", ", openPorts)}"
            };

            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(webhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Scan results sent to Discord successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to send scan results to Discord. Status Code: {response.StatusCode}");
            }
        }
    }

    static void LogScanResults(string logFileName, string target, List<int> openPorts, int timeoutMilliseconds, TimeSpan elapsedTime)
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFileName))
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
}
