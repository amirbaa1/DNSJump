using Spectre.Console;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace DNSJump.Service
{
    public class DnsNetworkService
    {
        public IReadOnlyList<string> GetDnsAddress(string adapterName)
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.Name == adapterName)
                .SelectMany(x => x.GetIPProperties().DnsAddresses)
                .Where(ip => !IsPrivateAddress(ip))
                .Select(x => x.ToString())
                .ToList();
        }

        public async Task SetDns(string adapterName, string dns1, string dns2)
        {
            await RunPowerShell($"Set-DnsClientServerAddress -InterfaceAlias '{adapterName}' -ServerAddresses {dns1},{dns2}");
        }

        public async Task ClearDns(string adapterName)
        {
            await RunPowerShell($"Set-DnsClientServerAddress -InterfaceAlias '{adapterName}' -ResetServerAddresses");
        }


        private static async Task RunPowerShell(string cmd)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = cmd,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            using var process = Process.Start(psi);
            var outprint = await process.StandardOutput.ReadToEndAsync();
            var errorprint = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(errorprint))
            {
                AnsiConsole.MarkupLine($"[red]Error: {Markup.Escape(errorprint)}[/]");
            }
        }

        private static bool IsPrivateAddress(IPAddress ip)
        {
            var b = ip.GetAddressBytes();
            return b[0] == 10 || (b[0] == 192 && b[1] == 168);
        }
    }
}
