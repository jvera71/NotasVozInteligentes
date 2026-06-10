namespace NotasVozInteligentes.Models;

public class DocumentoMarkdown
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset FechaModificacion { get; set; }
}
