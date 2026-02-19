namespace DNSJump.Model
{
    public record DnsEntry(string Name, string Primary, string Secondary)
    {
        public static DnsEntry TryParse(string line)
        {
            var parts = line.Split(',');
            return parts.Length == 3
                ? new DnsEntry(parts[0].Trim(), parts[1].Trim(), parts[2].Trim())
                : null;
        }

        public override string ToString() => $"{Name},{Primary},{Secondary}";

    }
}
