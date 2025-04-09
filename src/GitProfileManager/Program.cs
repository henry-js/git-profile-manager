using GitProfileManager.Services;
using Velopack;

VelopackApp.Build().Run();

MyServiceProvider sp = new();

ConsoleApp.ServiceProvider = sp;
var app = ConsoleApp.Create();
app.Add<ActivationCommands>();
app.Add<ProfileCommands>("profile");
await app.RunAsync(args);