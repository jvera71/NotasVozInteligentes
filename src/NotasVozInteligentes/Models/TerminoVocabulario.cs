namespace NotasVozInteligentes.Models;

public class TerminoVocabulario
{
    public Guid Id { get; set; }
    public string Termino { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
}
