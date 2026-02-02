using Ivy.VMLifecycle.Frontend.Apps;
using Ivy.VMLifecycle.Frontend.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();

#if DEBUG
server.UseHotReload();
#endif

server.Services.AddSingleton<IVMService, VMService>();
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

var chromeSettings = new ChromeSettings().DefaultApp<HomeApp>().UseTabs(preventDuplicates: true);
await server.RunAsync();
