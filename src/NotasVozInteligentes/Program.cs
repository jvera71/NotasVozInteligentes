using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Components;
using NotasVozInteligentes.Data;
using NotasVozInteligentes.Endpoints;
using NotasVozInteligentes.Services;

namespace NotasVozInteligentes
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddDbContext<AppDbContext>(o =>
                o.UseSqlite(builder.Configuration.GetConnectionString("Default")
                    ?? "Data Source=App_Data/notas.db"));

            builder.Services.AddSingleton<IAudioStorage, FileSystemAudioStorage>();
            builder.Services.AddScoped<IConversionService, ConversionService>();
            builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .AddStandardResilienceHandler(o =>
            {
                // La conversión global puede tardar varios minutos con muchos audios.
                o.AttemptTimeout.Timeout = TimeSpan.FromMinutes(4);
                o.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(5);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(8);
            });

            var app = builder.Build();

            // Aplica las migraciones pendientes (instancia única, ver docs/ARQUITECTURA.md §4.4).
            Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
            using (var scope = app.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();

            app.MapNotasEndpoints();
            app.MapDocumentosEndpoints();
            app.MapVocabularioEndpoints();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
