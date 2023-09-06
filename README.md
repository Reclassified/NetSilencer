# NetSilencer - Multi-Threaded Port Scanner

**NetSilencer** is a multi-threaded port scanner written in C# that empowers network administrators and security enthusiasts to quickly and efficiently scan target hosts or IP addresses for open ports.

## Introduction

NetSilencer is designed to streamline the process of identifying open ports on a network, making it an essential tool for:

- Network administrators managing network security.
- Security professionals conducting vulnerability assessments.
- Anyone interested in exploring the security of their network.

## Features

- **Multi-Threaded Scanning**: NetSilencer employs multi-threading to scan multiple ports concurrently, significantly reducing scanning time and increasing efficiency.

- **Custom Port Range**: Specify a custom port or range of ports to scan using the `-p` option, giving you flexibility in your scanning operations.

- **User-Friendly Output**: NetSilencer provides clear and user-friendly output, making it easy to identify open ports at a glance.

- **Robust Error Handling**: Even when the target is down or unreachable, NetSilencer handles errors gracefully, ensuring reliable performance.

## Usage

1. Clone this repository.
2. Compile the program using your preferred C# development environment.
3. Run the program, specifying the target's hostname or IP address.

To scan a target for open ports, simply run the executable and provide the target's hostname or IP address. By default, NetSilencer scans common ports from 1 to 1024. You can specify a custom port or range of ports using the `-p` option.

### Examples

Scan a specific port (e.g., port 80):

NetSilencer -p 80 example.com

Scan a range of ports (e.g., ports 20 to 100):

NetSilencer -p 20-100 example.com

Specify a custom timeout (e.g., 2000 milliseconds):

NetSilencer -p 80 -t 2000 example.com

## Disclaimer
Use this tool responsibly and only on networks and systems you have permission to scan. Unauthorized scanning may violate legal and ethical guidelines.
