using DNSJump.Model;
using DNSJump.Service;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace DNSJump.UI
{
    public class DnsMenu(DnsStoreService dnsStageService, DnsNetworkService dnsNetworkService)
    {
        public async Task RnuAsync()
        {
            while (true)
            {
                Console.Clear();
                ShowHeader();

                var action = Prompt(["Create DNS", "List DNS", "Network", "Exit"]);
                var result = action switch
                {
                    "Create DNS" => await HandleCreateAsync(),
                    "List DNS" => await HandleListAsync(),
                    "Network" => await HandleNetworkAsync(),
                    _ => false

                };
            }
        }

        private static void ShowHeader()
        {
            Console.Clear();
            AnsiConsole.Write(
                new FigletText("DNS Jump")
                    .Centered()
                    .Color(Color.Aqua)
            );
            AnsiConsole.Write(
                new Rule("[grey]Fast & Easy DNS Manager[/]")
                    .RuleStyle(Style.Parse("aqua"))
                    .Centered()
            );
            AnsiConsole.WriteLine();
        }

        private async Task<bool> HandleNetworkAsync()
        {
            while (true)
            {
                ShowHeader();

                var adapter = SelectAdaptor();
                if (adapter is null) return true;

                while (true)
                {

                    Console.Clear();
                    var address = dnsNetworkService.GetDnsAddress(adapter.Name);

                    //Table
                    TableRenderer.ShowDnsAddress(address);

                    string[] ope = address.Count == 0
                     ? ["Add", "Import", "Back"]
                     : ["Edit", "Import", "Clear", "Back"];

                    var action = Prompt(ope);
                    switch (action)
                    {
                        case "Add" or "Edit":
                            var dns1 = AskValidDnsOrBack("Primary DNS");
                            if (dns1 is null) continue;

                            var dns2 = AskValidDnsOrBack("Secondary DNS");
                            if (dns1 is null) continue;

                            await AnsiConsole.Status().StartAsync("Applying...",
                                _ => dnsNetworkService.SetDns(adapter.Name, dns1, dns2));
                            break;

                        case "Import":
                            var entry = SelectDnsEntry();
                            if (entry is null) continue;

                            await AnsiConsole.Status().StartAsync("Applying...",
                                _ => dnsNetworkService.SetDns(adapter.Name, entry.Primary, entry.Secondary));
                            break;

                        case "Clear":
                            await AnsiConsole.Status().StartAsync("Clearing...",
                                _ => dnsNetworkService.ClearDns(adapter.Name));
                            break;

                        case "Back":
                            goto selectAdapter;
                    }
                    Console.Clear();
                    var updated = dnsNetworkService.GetDnsAddress(adapter.Name);

                    if (updated.Count == 0)
                        AnsiConsole.MarkupLine("[green]✓ DNS cleared successfully.[/]\n");
                    else
                        AnsiConsole.MarkupLine("[green]✓ DNS updated successfully.[/]\n");

                    TableRenderer.ShowDnsAddress(updated);

                    AnsiConsole.MarkupLine("\n[grey]Press any key...[/]");
                    Console.ReadKey(true);
                    continue;
                }
            selectAdapter:
                Console.Clear();
            }
            return true;
        }

        private async Task<bool> HandleCreateAsync()
        {
            var name = AskOrBack("DNS Name");
            if (name is null) return false;
            var dns1 = AskValidDnsOrBack("Primary DNS");
            if (name is null) return false;
            var dns2 = AskValidDnsOrBack("Secondary DNS");
            if (name is null) return false;

            dnsStageService.Add(new DnsEntry(name, dns1, dns2));
            AnsiConsole.MarkupLine("[green]DNS added successfully.[/]");
            return true;
        }

        private static string? AskOrBack(string label)
        {
            var input = AnsiConsole.Ask<string>($"[yellow]{label} (or 'back'):[/]");
            return input.Trim().ToLower() == "back" ? null : input;
        }

        private static string? AskValidDnsOrBack(string label)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>($"[yellow]{label} (or 'back'):[/]");
                if (input.Trim().ToLower() == "back") return null;
                if (IsDnsValid(input)) return input;
                AnsiConsole.MarkupLine("[red]Invalid format. Example: 8.8.8.8[/]");
            }
        }

        private async Task<bool> HandleListAsync()
        {
            var entries = dnsStageService.GetAll();

            if (entries.Count == 0)
                AnsiConsole.MarkupLine("[red]No DNS entries found.[/]");
            else
                TableRenderer.ShowDnsEntries(entries);

            var action = Prompt(["Add DNS", "Back"]);
            if (action == "Add DNS") await HandleCreateAsync();
            return true;
        }

        private static string Prompt(string[] choices) =>
            AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .HighlightStyle(new Style(Color.Yellow, decoration: Decoration.Bold))
                .AddChoices(choices)
                );

        private static string Ask(string label) =>
            AnsiConsole.Ask<string>($"[yellow]{label}:[/]");

        private static string AskValidDns(string label)
        {
            while (true)
            {
                var input = Ask(label);
                if (IsDnsValid(input))
                {
                    return input;
                }
                AnsiConsole.MarkupLine("[red]Invalid format. Example: 8.8.8.8[/]");
            }
        }

        private static bool IsDnsValid(string ip)
        {
            var r = Regex.IsMatch(ip, @"^((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)$");
            return r;
        }

        private NetworkInterface SelectAdaptor()
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces().ToList();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]network interface[/]")
                    .AddChoices(adapters.Select(x =>
                    {
                        var status = x.OperationalStatus == OperationalStatus.Up
                            ? "[green]▲ UP[/]"
                            : "[red]▼ DOWN[/]";
                        return $"{status} {x.Description} [grey]({x.Name})[/]";
                    }).Append("Back"))
            );

            if (choice == "Back") return null;

            return adapters.First(x => choice.Contains(x.Description));
        }

        private DnsEntry? SelectDnsEntry()
        {
            var entries = dnsStageService.GetAll();
            if (entries.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No entries found.[/]");
                AnsiConsole.MarkupLine("\n[grey]Press any key to go back...[/]");
                Console.ReadKey(true);

                return null;
            }

            var options = entries.Select(e => $"{e.Name} - {e.Primary} / {e.Secondary}").Append("Back").ToList();
            var selected = Prompt([.. options]);
            if (selected == "Back") return null;

            return entries.First(e => selected.StartsWith(e.Name));
        }

    }
}
