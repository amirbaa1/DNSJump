using DNSJump.Model;
using Spectre.Console;

namespace DNSJump.UI
{
    public class TableRenderer
    {
        public static void ShowDnsAddress(IReadOnlyList<string> addresses)
        {
            var table = CreateTable("Number", "DNS Address");

            for (int i = 0; i < addresses.Count; i++)
                table.AddRow($@"[[{i + 1}]]", addresses[i]);

            AnsiConsole.Write(table);
        }
        public static void ShowDnsEntries(IReadOnlyList<DnsEntry> entries)
        {
            var table = CreateTable("Name", "Primary", "Secondary");

            foreach (var e in entries)
                table.AddRow(e.Name, e.Primary, e.Secondary);

            AnsiConsole.Write(table);
        }

        private static Table CreateTable(params string[] columns)
        {
            var table = new Table().RoundedBorder().BorderColor(Color.Aqua);

            foreach (var col in columns)
                table.AddColumn(new TableColumn(col).Centered());

            return table;
        }
    }
}
