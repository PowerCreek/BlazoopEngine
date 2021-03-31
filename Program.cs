using System.Threading.Tasks;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.ElementContexts;
using Blazoop.Source.NodeContexts;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blazoop
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            
            builder.Services.AddScoped<IServiceData, ServiceData>();
            builder.Services.AddScoped<OperationManager>();
            
            builder.Services.AddScoped<RootNode>();
            
            await builder.Build().RunAsync();

        }
    }
}