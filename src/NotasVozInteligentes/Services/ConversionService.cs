using System.Text;
using Microsoft.EntityFrameworkCore;
using NotasVozInteligentes.Data;
using NotasVozInteligentes.Models;

namespace NotasVozInteligentes.Services;

public class ConversionService(
    AppDbContext db,
    IAudioStorage audioStorage,
    IGeminiService gemini,
    ILogger<ConversionService> logger) : IConversionService
{
    public async Task<Guid> ConvertirAsync(CancellationToken ct = default)
    {
        if (await db.NotasVoz.AnyAsync(n => n.Estado == EstadoNota.EnProceso, ct))
            throw new ConversionEnCursoException();

        // SQLite no soporta ORDER BY sobre DateTimeOffset: se ordena en memoria.
        var notas = (await db.NotasVoz
            .Where(n => n.Estado == EstadoNota.PendienteDeConversion)
            .ToListAsync(ct))
            .OrderBy(n => n.FechaCreacion)
            .ToList();

        if (notas.Count == 0)
            throw new SinNotasPendientesException();

        foreach (var nota in notas)
            nota.Estado = EstadoNota.EnProceso;
        await db.SaveChangesAsync(ct);

        try
        {
            var glosario = await db.Vocabulario.OrderBy(t => t.Termino).ToListAsync(ct);

            var audios = new List<AudioNota>(notas.Count);
            foreach (var nota in notas)
                audios.Add(new AudioNota(
                    await audioStorage.LeerAsync(nota.RutaAudio, ct),
                    nota.MimeType,
                    nota.FechaCreacion));

            var resultado = await gemini.ProcesarNotasAsync(audios, glosario, ct);

            var ahora = DateTimeOffset.Now;
            var documento = new DocumentoMarkdown
            {
                Id = Guid.NewGuid(),
                Titulo = $"Notas — {ahora:dd/MM/yyyy HH:mm}",
                Contenido = RenderizarMarkdown(resultado, ahora),
                FechaCreacion = ahora,
                FechaModificacion = ahora
            };

            db.Documentos.Add(documento);
            db.NotasVoz.RemoveRange(notas);
            await db.SaveChangesAsync(ct);

            foreach (var nota in notas)
                audioStorage.Eliminar(nota.RutaAudio);

            logger.LogInformation(
                "Conversión completada: {NumNotas} notas → documento {DocumentoId}",
                notas.Count, documento.Id);

            return documento.Id;
        }
        catch
        {
            // La conversión falló: las notas vuelven a la cola y no se borra nada.
            foreach (var nota in notas)
                nota.Estado = EstadoNota.PendienteDeConversion;
            await db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private static string RenderizarMarkdown(ResultadoConversion resultado, DateTimeOffset fecha)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Notas — {fecha:dd/MM/yyyy}");

        foreach (var proyecto in resultado.Proyectos)
        {
            sb.AppendLine();
            sb.AppendLine($"## Proyecto {proyecto.Nombre}");
            foreach (var elemento in proyecto.Elementos)
            {
                sb.AppendLine();
                sb.AppendLine($"### {elemento.Nombre}");
                foreach (var tarea in elemento.Tareas)
                    sb.AppendLine($"- {tarea}");
            }
        }

        if (resultado.SinClasificar.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Sin clasificar");
            foreach (var item in resultado.SinClasificar)
                sb.AppendLine($"- {item}");
        }

        return sb.ToString();
    }
}
