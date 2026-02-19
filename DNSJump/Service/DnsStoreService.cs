using DNSJump.Model;
using System.Reflection;

namespace DNSJump.Service
{
    public class DnsStoreService
    {
        private readonly string _filePath;

        public DnsStoreService()
        {
            var folder = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "dns"
            );
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "dns.txt");
        }

        public IReadOnlyList<DnsEntry> GetAll()
        {
            if (!File.Exists(_filePath)) return [];

            return File.ReadAllLines(_filePath)
                .Select(DnsEntry.TryParse)
                .Where(e => e is not null)
                .ToList()!;
        }

        public void Add(DnsEntry entry)
            => File.AppendAllText(_filePath, entry + Environment.NewLine);
    }
}
