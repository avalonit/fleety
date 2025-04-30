using AITrackerAgent.Interfaces;
using AITrackerAgent.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

//var builder = FunctionsApplication.CreateBuilder(args);
//builder.ConfigureFunctionsWebApplication();
//builder.Build().Run();
//builder.Build();

var host = new HostBuilder();

host.ConfigureFunctionsWebApplication()
    .ConfigureServices(builder =>
    {
        builder.AddTransient<ICommunicationService, CommunicationService>();
        builder.AddTransient<IAddressService, AddressServices>();
        builder.AddTransient<IAgentService, AgentService>();
    });

host.Build().Run();

