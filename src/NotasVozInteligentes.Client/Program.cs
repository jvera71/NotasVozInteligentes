using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NotasVozInteligentes.Client.Services;

namespace NotasVozInteligentes.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped(_ => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
            builder.Services.AddScoped<ApiClient>();

            await builder.Build().RunAsync();
        }
    }
}
