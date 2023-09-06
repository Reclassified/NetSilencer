##NetSilencer - Multi-Threaded Port Scanner

NetSilencer is a simple yet powerful multi-threaded port scanner written in C#. It allows you to quickly scan a target host or IP address for open ports, making it a valuable tool for network administrators and security enthusiasts.

##Features:

Multi-Threaded Scanning: NetSilencer uses multi-threading to scan multiple ports concurrently, significantly reducing scanning time.

Custom Port Range: Specify a custom port or range of ports to scan using the -p option.

User-Friendly Output: The tool displays a list of open ports for easy identification.

Error Handling: NetSilencer gracefully handles cases where the target is down or unreachable.


##Usage

To scan a target for open ports, simply run the executable and provide the target's hostname or IP address. By default, NetSilencer scans common ports from 1 to 1024. You can specify a custom port or range of ports using the -p option.

NetSilencer [-p <port>] <target>

#Example

Scan a target for open ports:
NetSilencer example.com

Scan a specific port (e.g., port 80):
NetSilencer -p 80 example.com
