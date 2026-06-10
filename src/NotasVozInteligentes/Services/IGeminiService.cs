using NotasVozInteligentes.Models;

namespace NotasVozInteligentes.Services;

public record AudioNota(byte[] Contenido, string MimeType, DateTimeOffset FechaCreacion);

public record ElementoProyecto(string Nombre, string Tipo, List<string> Tareas);

public record ProyectoNotas(string Nombre, List<ElementoProyecto> Elementos);

public record ResultadoConversion(List<ProyectoNotas> Proyectos, List<string> SinClasificar);

public interface IGeminiService
{
    Task<ResultadoConversion> ProcesarNotasAsync(
        IReadOnlyList<AudioNota> audios,
        IReadOnlyList<TerminoVocabulario> glosario,
        CancellationToken ct = default);
}
