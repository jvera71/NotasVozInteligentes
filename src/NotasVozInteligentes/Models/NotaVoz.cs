namespace NotasVozInteligentes.Models;

public enum EstadoNota
{
    PendienteDeConversion = 0,
    EnProceso = 1
}

public class NotaVoz
{
    public Guid Id { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public string RutaAudio { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public double? DuracionSegundos { get; set; }
    public EstadoNota Estado { get; set; } = EstadoNota.PendienteDeConversion;
}
