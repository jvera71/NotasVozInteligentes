namespace NotasVozInteligentes.Client.Shared;

public record NotaVozDto(
    Guid Id,
    DateTimeOffset FechaCreacion,
    string MimeType,
    double? DuracionSegundos,
    string Estado);

public record DocumentoResumenDto(
    Guid Id,
    string Titulo,
    DateTimeOffset FechaCreacion,
    DateTimeOffset FechaModificacion);

public record DocumentoDto(
    Guid Id,
    string Titulo,
    string Contenido,
    DateTimeOffset FechaCreacion,
    DateTimeOffset FechaModificacion);

public record ActualizarDocumentoRequest(string Titulo, string Contenido);

public record TerminoDto(
    Guid Id,
    string Termino,
    string? Descripcion,
    DateTimeOffset FechaCreacion);

public record GuardarTerminoRequest(string Termino, string? Descripcion);

public record ConversionResultadoDto(Guid DocumentoId);
