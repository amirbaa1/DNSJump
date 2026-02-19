// See https://aka.ms/new-console-template for more information
using DNSJump.Service;
using DNSJump.UI;
using System.Security.Principal;

//Console.WriteLine("Hello, World!");

if (!IsAdministrator())
{

    Console.WriteLine("⚠ Please run this program as Administrator!");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
    return;
}
else
{
    var storeService = new DnsStoreService();
    var networkService = new DnsNetworkService();
    var menu = new DnsMenu(storeService, networkService);

    await menu.RnuAsync();
}



static bool IsAdministrator()
{
    WindowsIdentity identity = WindowsIdentity.GetCurrent();
    WindowsPrincipal principal = new WindowsPrincipal(identity);
    return principal.IsInRole(WindowsBuiltInRole.Administrator);
}
