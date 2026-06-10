namespace NotasVozInteligentes.Services;

public interface IAudioStorage
{
    Task<string> GuardarAsync(Stream contenido, string mimeType, CancellationToken ct = default);
    Stream Abrir(string rutaRelativa);
    Task<byte[]> LeerAsync(string rutaRelativa, CancellationToken ct = default);
    void Eliminar(string rutaRelativa);
}
