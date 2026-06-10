using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Client.Shared;
using NotasVozInteligentes.Data;
using NotasVozInteligentes.Models;

namespace NotasVozInteligentes.Endpoints;

public static class VocabularioEndpoints
{
    public static void MapVocabularioEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/vocabulario");

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
            await db.Vocabulario
                .OrderBy(t => t.Termino)
                .Select(t => new TerminoDto(t.Id, t.Termino, t.Descripcion, t.FechaCreacion))
                .ToListAsync(ct));

        grupo.MapPost("/", async (GuardarTerminoRequest peticion, AppDbContext db, CancellationToken ct) =>
        {
            var termino = peticion.Termino.Trim();
            if (string.IsNullOrEmpty(termino))
                return Results.BadRequest("El término no puede estar vacío.");

            if (await ExisteDuplicado(db, termino, null, ct))
                return Results.Conflict($"El término '{termino}' ya existe.");

            var entidad = new TerminoVocabulario
            {
                Id = Guid.NewGuid(),
                Termino = termino,
                Descripcion = Normalizar(peticion.Descripcion),
                FechaCreacion = DateTimeOffset.Now
            };
            db.Vocabulario.Add(entidad);
            await db.SaveChangesAsync(ct);

            return Results.Created(
                $"/api/vocabulario/{entidad.Id}",
                new TerminoDto(entidad.Id, entidad.Termino, entidad.Descripcion, entidad.FechaCreacion));
        });

        grupo.MapPut("/{id:guid}", async (Guid id, GuardarTerminoRequest peticion, AppDbContext db, CancellationToken ct) =>
        {
            var entidad = await db.Vocabulario.FindAsync([id], ct);
            if (entidad is null)
                return Results.NotFound();

            var termino = peticion.Termino.Trim();
            if (string.IsNullOrEmpty(termino))
                return Results.BadRequest("El término no puede estar vacío.");

            if (await ExisteDuplicado(db, termino, id, ct))
                return Results.Conflict($"El término '{termino}' ya existe.");

            entidad.Termino = termino;
            entidad.Descripcion = Normalizar(peticion.Descripcion);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        grupo.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var entidad = await db.Vocabulario.FindAsync([id], ct);
            if (entidad is null)
                return Results.NotFound();

            db.Vocabulario.Remove(entidad);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }

    private static Task<bool> ExisteDuplicado(AppDbContext db, string termino, Guid? exceptoId, CancellationToken ct) =>
        db.Vocabulario.AnyAsync(
            t => t.Termino.ToLower() == termino.ToLower() && (exceptoId == null || t.Id != exceptoId),
            ct);

    private static string? Normalizar(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
