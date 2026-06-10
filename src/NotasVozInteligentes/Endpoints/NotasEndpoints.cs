using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Client.Shared;
using NotasVozInteligentes.Data;
using NotasVozInteligentes.Models;
using NotasVozInteligentes.Services;

namespace NotasVozInteligentes.Endpoints;

public static class NotasEndpoints
{
    public static void MapNotasEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/notas");

        grupo.MapPost("/", async (HttpRequest request, AppDbContext db, IAudioStorage storage, CancellationToken ct) =>
        {
            var form = await request.ReadFormAsync(ct);
            var fichero = form.Files.GetFile("audio");
            if (fichero is null || fichero.Length == 0)
                return Results.BadRequest("Falta el fichero de audio.");

            double? duracion = double.TryParse(form["duracionSegundos"], out var d) ? d : null;

            await using var stream = fichero.OpenReadStream();
            var ruta = await storage.GuardarAsync(stream, fichero.ContentType, ct);

            var nota = new NotaVoz
            {
                Id = Guid.NewGuid(),
                FechaCreacion = DateTimeOffset.Now,
                RutaAudio = ruta,
                MimeType = fichero.ContentType,
                DuracionSegundos = duracion,
                Estado = EstadoNota.PendienteDeConversion
            };
            db.NotasVoz.Add(nota);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/notas/{nota.Id}", ADto(nota));
        })
        .DisableAntiforgery();

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            // SQLite no soporta ORDER BY sobre DateTimeOffset: se ordena en memoria.
            var notas = await db.NotasVoz.ToListAsync(ct);
            return notas.OrderBy(n => n.FechaCreacion).Select(ADto).ToList();
        });

        grupo.MapGet("/{id:guid}/audio", async (Guid id, AppDbContext db, IAudioStorage storage, CancellationToken ct) =>
        {
            var nota = await db.NotasVoz.FindAsync([id], ct);
            return nota is null
                ? Results.NotFound()
                : Results.Stream(storage.Abrir(nota.RutaAudio), nota.MimeType);
        });

        grupo.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, IAudioStorage storage, CancellationToken ct) =>
        {
            var nota = await db.NotasVoz.FindAsync([id], ct);
            if (nota is null)
                return Results.NotFound();

            db.NotasVoz.Remove(nota);
            await db.SaveChangesAsync(ct);
            storage.Eliminar(nota.RutaAudio);
            return Results.NoContent();
        });

        grupo.MapPost("/convertir", async (IConversionService conversion, CancellationToken ct) =>
        {
            try
            {
                var documentoId = await conversion.ConvertirAsync(ct);
                return Results.Ok(new ConversionResultadoDto(documentoId));
            }
            catch (ConversionEnCursoException)
            {
                return Results.Conflict("Ya hay una conversión en curso.");
            }
            catch (SinNotasPendientesException)
            {
                return Results.BadRequest("No hay notas pendientes de conversión.");
            }
        });
    }

    private static NotaVozDto ADto(NotaVoz n) =>
        new(n.Id, n.FechaCreacion, n.MimeType, n.DuracionSegundos, n.Estado.ToString());
}
