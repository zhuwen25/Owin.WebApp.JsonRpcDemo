namespace JsonRpcServer;

public class JsonRpcStartup
{
    public JsonRpcStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        //services.AddTransient<GreeterServer>();
        services.AddTransient<HypervisorService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        //app.usehtt
        app.UseRouting();
        app.UseAuthorization();
        app.UseWebSockets();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
