using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class NetSilencer
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to NetSilencer!");
        Console.Title = "NetSilencer by BlasterEngine";
        string target = string.Empty;
        int[] portsToScan = GetDefaultPortsToScan(); // Default to common ports

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-p" && i + 1 < args.Length && int.TryParse(args[i + 1], out int customPort))
            {
                portsToScan = new[] { customPort };
                i++; // Skip the next argument (the custom port)
            }
            else
            {
                target = args[i];
            }
        }

        if (string.IsNullOrEmpty(target))
        {
            Console.WriteLine("Usage: NetSilencer [-p <port>] <target>");
            return;
        }

        Console.WriteLine($"Scanning target: {target}");

        List<int> openPorts = new List<int>();

        try
        {
            // Use Parallel.ForEach to scan ports concurrently
            Parallel.ForEach(portsToScan, port =>
            {
                if (IsPortOpen(target, port))
                {
                    openPorts.Add(port);
                }
            });

            if (openPorts.Count > 0)
            {
                Console.WriteLine("Open ports:");
                foreach (int port in openPorts)
                {
                    Console.WriteLine($"Port {port}");
                }
            }
            else
            {
                Console.WriteLine("No open ports found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("The target may be down or unreachable.");
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

    static bool IsPortOpen(string host, int port)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(host, port);
                return true;
            }
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
