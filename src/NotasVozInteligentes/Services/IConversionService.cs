namespace NotasVozInteligentes.Services;

public interface IConversionService
{
    /// <summary>
    /// Procesa globalmente todas las notas pendientes con Gemini y genera un documento Markdown.
    /// </summary>
    /// <returns>El Id del documento generado.</returns>
    /// <exception cref="ConversionEnCursoException">Ya hay una conversión en curso.</exception>
    /// <exception cref="SinNotasPendientesException">No hay notas pendientes.</exception>
    Task<Guid> ConvertirAsync(CancellationToken ct = default);
}

public class ConversionEnCursoException : Exception;

public class SinNotasPendientesException : Exception;
