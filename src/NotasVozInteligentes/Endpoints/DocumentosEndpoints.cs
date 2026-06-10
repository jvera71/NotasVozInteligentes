using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Client.Shared;
using NotasVozInteligentes.Data;

namespace NotasVozInteligentes.Endpoints;

public static class DocumentosEndpoints
{
    public static void MapDocumentosEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/documentos");

        grupo.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            // SQLite no soporta ORDER BY sobre DateTimeOffset: se ordena en memoria.
            var documentos = await db.Documentos
                .Select(d => new DocumentoResumenDto(d.Id, d.Titulo, d.FechaCreacion, d.FechaModificacion))
                .ToListAsync(ct);
            return documentos.OrderByDescending(d => d.FechaCreacion).ToList();
        });

        grupo.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var doc = await db.Documentos.FindAsync([id], ct);
            return doc is null
                ? Results.NotFound()
                : Results.Ok(new DocumentoDto(doc.Id, doc.Titulo, doc.Contenido, doc.FechaCreacion, doc.FechaModificacion));
        });

        grupo.MapPut("/{id:guid}", async (Guid id, ActualizarDocumentoRequest peticion, AppDbContext db, CancellationToken ct) =>
        {
            var doc = await db.Documentos.FindAsync([id], ct);
            if (doc is null)
                return Results.NotFound();

            doc.Titulo = peticion.Titulo;
            doc.Contenido = peticion.Contenido;
            doc.FechaModificacion = DateTimeOffset.Now;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });

        grupo.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var doc = await db.Documentos.FindAsync([id], ct);
            if (doc is null)
                return Results.NotFound();

            db.Documentos.Remove(doc);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        });
    }
}
