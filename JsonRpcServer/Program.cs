using JsonRpcServer;

var builder = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.UseStartup<JsonRpcStartup>();
});

// Add services to the container.
var app = builder.Build();
app.Run();
